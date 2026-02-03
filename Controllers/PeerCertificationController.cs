using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic; 
using System.Text.Json;

namespace HIVTraining_Vue.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PeerCertificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PeerCertificationController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;

        }

        private static readonly string[] AllowedExt = new[] { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg" };
        private const long MaxUploadBytes = 15 * 1024 * 1024; // 15MB

        private string UploadRoot => Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "PeerUploads");

        // safer file name
        private static string SafeFileName(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');

            // also prevent weird traversal attempts
            fileName = fileName.Replace("..", "_");
            return fileName;
        }

        [HttpGet("step5-doc-types")]
        public async Task<IActionResult> GetStep5DocTypes()
        {
            int[] step5Ids = new[] { 3, 2, 4, 8, 6, 7 };

            var fallbackNames = new Dictionary<int, string>
    {
        { 3, "Code of Ethics" },
        { 2, "Resume" },
        { 4, "Foundational Training Certificate" },
        { 8, "Safe Talk Suicide Alertness Training Certificate" },
        { 6, "Other Certificates / Diplomas" },
        { 7, "Supervisor Practicum Evaluation Form" }
    };

            var rows = await _context.LkPeerDocTypes
                .AsNoTracking()
.Where(x => step5Ids.Contains(x.PeerDocId) && x.Active == true).Select(x => new
                {
                    peerDocId = x.PeerDocId,
                    name = x.Name,
                    description = x.Description,
                    docAbbrev = x.DocAbbrev,
                    required = (x.PeerDocId == 2 || x.PeerDocId == 3 || x.PeerDocId == 7),
                    active = x.Active
                })
                .ToListAsync();

            // build final list in the required order; fallback if missing/inactive
            var ordered = step5Ids.Select(id =>
            {
                var db = rows.FirstOrDefault(r => r.peerDocId == id && r.active == true);

                return db ?? new
                {
                    peerDocId = id,
                    name = fallbackNames.TryGetValue(id, out var nm) ? nm : $"Document {id}",
                    description = (string?)null,
                    docAbbrev = (string?)null,
                    required = (id == 2 || id == 3 || id == 7),
                    active = true
                };
            }).ToList();

            return Ok(ordered);
        }
        [HttpGet("ethics/{userId:guid}")]
        public async Task<IActionResult> GetEthics(Guid userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound(new { message = "User not found" });

            var peer = await _context.PeerUsers.AsNoTracking().FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);
            if (peer == null) return Ok(new { signed = false });

            // peerDocId 3 = Ethics
            var doc = await _context.PeerDocs.AsNoTracking()
                .Where(d => d.PeerSysId == peer.PeerSysId && d.PeerDocId == 3 && d.Active == true)
                .OrderByDescending(d => d.DateUpload)
                .FirstOrDefaultAsync();

            if (doc == null) return Ok(new { signed = false });

            // DocPath stores JSON string for ethics record
            string? signatureName = null;
            DateTime? signedAt = doc.DateUpload;

            try
            {
                // If DocPath contains JSON, parse it.
                if (!string.IsNullOrWhiteSpace(doc.DocPath) && doc.DocPath.TrimStart().StartsWith("{"))
                {
                    using var j = JsonDocument.Parse(doc.DocPath);
                    if (j.RootElement.TryGetProperty("signatureName", out var sn)) signatureName = sn.GetString();
                    if (j.RootElement.TryGetProperty("signedAt", out var sa) && DateTime.TryParse(sa.GetString(), out var dt))
                        signedAt = dt;
                }
            }
            catch { /* ignore parse errors */ }

            return Ok(new { signed = true, signedAt, signatureName });
        }

        private string EthicsPdfPath =>
            Path.Combine(_env.ContentRootPath, "app_data", "staticdocs", "peer-code-of-ethics.pdf");

        [HttpGet("ethics/pdf")]
        public IActionResult GetEthicsPdf()
        {
            var path = EthicsPdfPath;

            if (!System.IO.File.Exists(path))
                return NotFound(new { message = $"Ethics PDF not found at: {path}" });

            // Range enabled helps PDF viewers (seeking / fast load)
            return PhysicalFile(path, "application/pdf", enableRangeProcessing: true);
        }

        [HttpPost("ethics/{userId:guid}")]
        public async Task<IActionResult> SignEthics(Guid userId, [FromBody] JsonElement body)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound(new { message = "User not found" });

            var peer = await _context.PeerUsers.FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);
            if (peer == null) return BadRequest(new { message = "Peer record not found. Save Step 1 first." });

            static bool? GetBool(JsonElement e, string name)
            {
                if (!e.TryGetProperty(name, out var p)) return null;
                if (p.ValueKind == JsonValueKind.Null) return null;
                if (p.ValueKind == JsonValueKind.True) return true;
                if (p.ValueKind == JsonValueKind.False) return false;
                if (p.ValueKind == JsonValueKind.String && bool.TryParse(p.GetString(), out var b)) return b;
                return null;
            }

            static string? GetString(JsonElement e, string name)
            {
                if (!e.TryGetProperty(name, out var p)) return null;
                if (p.ValueKind == JsonValueKind.Null) return null;
                var s = p.GetString();
                return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
            }

            var agreed = GetBool(body, "agreed");
            var signatureName = GetString(body, "signatureName");

            if (agreed != true)
                return BadRequest(new { message = "Please confirm you agree to the Code of Ethics." });

            if (string.IsNullOrWhiteSpace(signatureName) || signatureName.Length < 3)
                return BadRequest(new { message = "Please type your full name to sign." });

            // deactivate previous ethics records (optional)
            var existing = await _context.PeerDocs
                .Where(d => d.PeerSysId == peer.PeerSysId && d.PeerDocId == 3 && d.Active == true)
                .ToListAsync();

            foreach (var d in existing)
            {
                d.Active = false;
                d.DateModify = DateTime.UtcNow;
            }

            var signedAt = DateTime.UtcNow;
            var payloadJson = JsonSerializer.Serialize(new
            {
                signatureName,
                signedAt = signedAt.ToString("o")
            });

            var ethicsDoc = new PeerDoc
            {
                PeerSysId = peer.PeerSysId,
                PeerDocId = 3,
                DocType = 3,
                // store JSON instead of a file path
                DocPath = payloadJson,
                DateUpload = signedAt,
                Active = true,
                UploadBy = user.Email ?? user.UserSysId.ToString(),
                Reviewed = false
            };

            _context.PeerDocs.Add(ethicsDoc);
            await _context.SaveChangesAsync();

            return Ok(new { signed = true, signedAt, signatureName });
        }

        // ==========================================================
        // GET Applicant Info (Users + PeerUsers)
        // ==========================================================
        [HttpGet("applicant-info/{userId:guid}")]
        public async Task<IActionResult> GetApplicantInfo(Guid userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var peer = await _context.PeerUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);

            // Track derived from existing flags (only one should be true)
            var track =
                (peer?.CertHiv == true) ? "HIV" :
                (peer?.CertHcv == true) ? "HCV" :
                (peer?.CertHr == true) ? "HR" :
                (peer?.CertPrep == true) ? "PREP" :
                (peer?.CertCriminalJustice == true) ? "CJ" :
                "";

            return Ok(new
            {
                user.UserId,
                user.UserSysId,
                user.FirstName,
                user.Mi,
                user.LastName,

                user.Email,
                user.AltEmail,

                user.Phone,
                user.AltPhone,
                user.CellPhone,

                user.WorkPhone,
                user.WorkPhoneExt,
                user.PrimaryCanText,
                user.AltCanText,

                user.Address,
                user.City,
                user.State,
                user.Zip,
                user.Country,

                user.Title,
                user.Organization,

                user.WorkSetting,
                user.Education,
                user.Ethnicity,
                user.Race,
                user.Occupation,
                user.YearsCurrentOccupation,

                user.PronounId,
                user.WorkLocationId,

                user.Adaneed,
                user.Adadetails,

                PeerSysId = peer != null ? (int?)peer.PeerSysId : null,
                Dob = peer?.Dob,
                Gender = peer?.Gender,
                AgencyAffilation = peer?.AgencyAffilation,

                CertificationTrack = track,

                ExperienceCommitment = peer?.ExperienceCommitment,
                ExperienceChallenges = peer?.ExperienceChallenges,
                ExperienceWhy = peer?.ExperienceWhy,
                SelfCare = peer?.SelfCare,

                SupvrOrgName = peer?.SupvrOrgName,
                SupvrFirstName = peer?.SupvrFirstName,
                SupvrLastName = peer?.SupvrLastName,
                SupvrContAddr1 = peer?.SupvrContAddr1,
                SupvrContAddr2 = peer?.SupvrContAddr2,
                SupvrContPhone = peer?.SupvrContPhone,
                SupvrContEmail = peer?.SupvrContEmail,
                ComplPracticum = peer?.ComplPracticum,
                ComplPracticumMin = peer?.ComplPracticumMin,
                PracticumBDate = peer?.PracticumBdate,
                PracticumEDate = peer?.PracticumEdate,

                RequiredCourses = peer?.RequiredCourses ?? false
            });
        }

        // ==========================================================
        // PUT Applicant Info (partial updates by step)
        // IMPORTANT: ONLY update fields that exist in payload
        // ==========================================================
        [HttpPut("applicant-info/{userId:guid}")]
        public async Task<IActionResult> SaveApplicantInfo(Guid userId, [FromBody] JsonElement body)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // ---------- helpers ----------
            static bool HasProp(JsonElement e, string name) =>
                e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out _);

            static string? GetString(JsonElement e, string name)
            {
                if (!e.TryGetProperty(name, out var p)) return null;
                if (p.ValueKind == JsonValueKind.Null) return null;
                var s = p.GetString();
                return string.IsNullOrWhiteSpace(s) ? null : s;
            }

            static int? GetInt(JsonElement e, string name)
            {
                if (!e.TryGetProperty(name, out var p)) return null;
                if (p.ValueKind == JsonValueKind.Null) return null;
                if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var v)) return v;
                if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var s)) return s;
                return null;
            }

            static bool? GetBool(JsonElement e, string name)
            {
                if (!e.TryGetProperty(name, out var p)) return null;
                if (p.ValueKind == JsonValueKind.Null) return null;
                if (p.ValueKind == JsonValueKind.True) return true;
                if (p.ValueKind == JsonValueKind.False) return false;
                if (p.ValueKind == JsonValueKind.String && bool.TryParse(p.GetString(), out var b)) return b;
                return null;
            }

            static DateTime? GetDate(JsonElement e, string name)
            {
                if (!e.TryGetProperty(name, out var p)) return null;
                if (p.ValueKind == JsonValueKind.Null) return null;
                if (p.ValueKind == JsonValueKind.String && DateTime.TryParse(p.GetString(), out var d)) return d;
                return null;
            }

            // =========================
            // Update Users (ONLY if property exists in payload)
            // =========================
            if (HasProp(body, "FirstName")) user.FirstName = GetString(body, "FirstName") ?? user.FirstName;
            if (HasProp(body, "Mi")) user.Mi = GetString(body, "Mi");
            if (HasProp(body, "LastName")) user.LastName = GetString(body, "LastName") ?? user.LastName;

            if (HasProp(body, "Email")) user.Email = GetString(body, "Email") ?? user.Email;
            if (HasProp(body, "AltEmail")) user.AltEmail = GetString(body, "AltEmail");

            if (HasProp(body, "Phone")) user.Phone = GetString(body, "Phone");
            if (HasProp(body, "AltPhone")) user.AltPhone = GetString(body, "AltPhone");
            if (HasProp(body, "CellPhone")) user.CellPhone = GetString(body, "CellPhone");

            if (HasProp(body, "WorkPhone")) user.WorkPhone = GetString(body, "WorkPhone");
            if (HasProp(body, "WorkPhoneExt")) user.WorkPhoneExt = GetString(body, "WorkPhoneExt");
            if (HasProp(body, "PrimaryCanText")) user.PrimaryCanText = GetBool(body, "PrimaryCanText");
            if (HasProp(body, "AltCanText")) user.AltCanText = GetBool(body, "AltCanText");

            if (HasProp(body, "Address")) user.Address = GetString(body, "Address");
            if (HasProp(body, "City")) user.City = GetString(body, "City");
            if (HasProp(body, "State")) user.State = GetString(body, "State");
            if (HasProp(body, "Zip")) user.Zip = GetString(body, "Zip");
            if (HasProp(body, "Country")) user.Country = GetString(body, "Country");

            if (HasProp(body, "Title")) user.Title = GetString(body, "Title");
            if (HasProp(body, "Organization")) user.Organization = GetString(body, "Organization");

            if (HasProp(body, "WorkSetting")) user.WorkSetting = GetInt(body, "WorkSetting");
            if (HasProp(body, "Education")) user.Education = GetInt(body, "Education");
            if (HasProp(body, "Ethnicity")) user.Ethnicity = GetInt(body, "Ethnicity");
            if (HasProp(body, "Race")) user.Race = GetInt(body, "Race");
            if (HasProp(body, "Occupation")) user.Occupation = GetInt(body, "Occupation");
            if (HasProp(body, "YearsCurrentOccupation")) user.YearsCurrentOccupation = GetInt(body, "YearsCurrentOccupation");

            if (HasProp(body, "PronounId")) user.PronounId = GetInt(body, "PronounId");
            if (HasProp(body, "WorkLocationId")) user.WorkLocationId = GetInt(body, "WorkLocationId");

            if (HasProp(body, "Adaneed")) user.Adaneed = GetBool(body, "Adaneed");

            // Only update Adadetails if the request includes it
            if (HasProp(body, "Adadetails"))
            {
                var adaDetails = GetString(body, "Adadetails");
                user.Adadetails = (user.Adaneed == true) ? adaDetails : null;
            }

            user.DateModified = DateTime.UtcNow;

            // =========================
            // Upsert PeerUsers
            // =========================
            var peer = await _context.PeerUsers.FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);
            if (peer == null)
            {
                peer = new PeerUser
                {
                    UserSysId = user.UserSysId,
                    DateCreate = DateTime.UtcNow,
                    Active = true,

                    RequiredCourses = false,
                    DisapprvEmailSent = false,

                    CertPrep = false,
                    CertCriminalJustice = false,

                    // Optional: set other Cert flags false by default too
                    CertHiv = false,
                    CertHcv = false,
                    CertHr = false
                };
                _context.PeerUsers.Add(peer);
            }

            // Step 1 fields stored in PeerUser (only update if present)
            if (HasProp(body, "Dob")) peer.Dob = GetDate(body, "Dob");
            if (HasProp(body, "Gender")) peer.Gender = GetInt(body, "Gender");
            if (HasProp(body, "AgencyAffilation")) peer.AgencyAffilation = GetString(body, "AgencyAffilation");

            // Step 2 lived experience (only update if present)
            if (HasProp(body, "ExperienceCommitment")) peer.ExperienceCommitment = GetString(body, "ExperienceCommitment");
            if (HasProp(body, "ExperienceChallenges")) peer.ExperienceChallenges = GetString(body, "ExperienceChallenges");
            if (HasProp(body, "ExperienceWhy")) peer.ExperienceWhy = GetString(body, "ExperienceWhy");
            if (HasProp(body, "SelfCare")) peer.SelfCare = GetBool(body, "SelfCare");

            // Step 3 required courses
            if (HasProp(body, "RequiredCourses"))
            {
                var reqCourses = GetBool(body, "RequiredCourses");
                if (reqCourses.HasValue)
                    peer.RequiredCourses = reqCourses.Value;
            }

            // Step 4 Supervisor / Practicum (only update if present)
            if (HasProp(body, "SupvrOrgName")) peer.SupvrOrgName = GetString(body, "SupvrOrgName");
            if (HasProp(body, "SupvrFirstName")) peer.SupvrFirstName = GetString(body, "SupvrFirstName");
            if (HasProp(body, "SupvrLastName")) peer.SupvrLastName = GetString(body, "SupvrLastName");
            if (HasProp(body, "SupvrContAddr1")) peer.SupvrContAddr1 = GetString(body, "SupvrContAddr1");
            if (HasProp(body, "SupvrContAddr2")) peer.SupvrContAddr2 = GetString(body, "SupvrContAddr2");
            if (HasProp(body, "SupvrContPhone")) peer.SupvrContPhone = GetString(body, "SupvrContPhone");
            if (HasProp(body, "SupvrContEmail")) peer.SupvrContEmail = GetString(body, "SupvrContEmail");

            if (HasProp(body, "ComplPracticum"))
            {
                var v = GetBool(body, "ComplPracticum");
                peer.ComplPracticum = v;
                if (v != true)
                {
                    // if not completed, clear dependent fields
                    peer.ComplPracticumMin = null;
                    peer.PracticumBdate = null;
                    peer.PracticumEdate = null;
                }
            }

            if (HasProp(body, "ComplPracticumMin")) peer.ComplPracticumMin = GetBool(body, "ComplPracticumMin");
            if (HasProp(body, "PracticumBDate")) peer.PracticumBdate = GetDate(body, "PracticumBDate");
            if (HasProp(body, "PracticumEDate")) peer.PracticumEdate = GetDate(body, "PracticumEDate");

            // Track flags (only update if present)
            if (HasProp(body, "CertificationTrack"))
            {
                var trackVal = GetString(body, "CertificationTrack") ?? "";

                // Only one track true at a time:
                peer.CertHiv = trackVal == "HIV";
                peer.CertHcv = trackVal == "HCV";
                peer.CertHr = trackVal == "HR";
                peer.CertPrep = trackVal == "PREP";
                peer.CertCriminalJustice = trackVal == "CJ";
            }

            peer.DateModify = DateTime.UtcNow;
            peer.Active = true;

            await _context.SaveChangesAsync();

            return await GetApplicantInfo(userId);
        }

        [HttpGet("uploads/{userId:guid}")]
        public async Task<IActionResult> GetUploads(Guid userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound(new { message = "User not found" });

            var peer = await _context.PeerUsers.AsNoTracking().FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);
            if (peer == null) return Ok(new { peerSysId = (int?)null, docs = Array.Empty<object>() });

            var docs = await (
                from d in _context.PeerDocs.AsNoTracking()
                join t in _context.LkPeerDocTypes.AsNoTracking()
                    on d.PeerDocId equals t.PeerDocId
                where d.PeerSysId == peer.PeerSysId
                      && d.Active == true
                      && t.Active == true
                orderby d.DateUpload descending
                select new
                {
                    d.PeerDocSysId,
                    d.PeerDocId,                // ✅ use this in UI filtering
                    docTypeName = t.Name,       // optional display
                    FileName = Path.GetFileName(d.DocPath),
                    d.DateUpload,
                    d.Reviewed
                }
            ).ToListAsync();

            return Ok(new { peerSysId = peer.PeerSysId, docs });
        }

        [HttpPost("uploads/{userId:guid}")]
        [RequestSizeLimit(MaxUploadBytes)]
        public async Task<IActionResult> Upload(Guid userId, [FromForm] IFormFile file, [FromForm] int docType)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            if (file.Length > MaxUploadBytes)
                return BadRequest(new { message = "File too large (max 15MB)." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExt.Contains(ext))
                return BadRequest(new { message = "Invalid file type. Allowed: pdf, doc, docx, png, jpg, jpeg." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound(new { message = "User not found" });

            var peer = await _context.PeerUsers.FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);
            if (peer == null) return BadRequest(new { message = "Peer record not found. Save Step 1 first." });

            // optional rule: allow only one file per docType by soft-deactivating previous
            var existing = await _context.PeerDocs
                .Where(d => d.PeerSysId == peer.PeerSysId && d.DocType == docType && d.Active == true)
                .ToListAsync();
            foreach (var d in existing)
            {
                d.Active = false;
                d.DateModify = DateTime.UtcNow;
            }

            Directory.CreateDirectory(UploadRoot);

            // folder per PeerSysId
            var folder = Path.Combine(UploadRoot, peer.PeerSysId.ToString());
            Directory.CreateDirectory(folder);

            var safeName = SafeFileName(Path.GetFileName(file.FileName));
            var storedName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, storedName);

            using (var stream = new FileStream(fullPath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            // ✅ Validate doc type exists and active (from Lk_Peer_Doc_Type)
            var dt = await _context.LkPeerDocTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PeerDocId == docType && x.Active == true);

            if (dt == null)
                return BadRequest(new { message = "Invalid document type." });
            var existingDocs = await _context.PeerDocs
    .Where(d => d.PeerSysId == peer.PeerSysId && d.PeerDocId == docType && d.Active == true)
    .ToListAsync();

            foreach (var d in existingDocs)
            {
                d.Active = false;
                d.DateModify = DateTime.UtcNow;
            }

            var doc = new PeerDoc
            {
                PeerSysId = peer.PeerSysId,

                //  FIX: category stored here
                PeerDocId = docType,

                // optional: keep DocType same if older code expects it
                DocType = docType,

                DocPath = fullPath,
                DateUpload = DateTime.UtcNow,
                Active = true,
                UploadBy = user.Email ?? user.UserSysId.ToString(),
                Reviewed = false
            };

            _context.PeerDocs.Add(doc);
            await _context.SaveChangesAsync();

            return await GetUploads(userId);
        }

        [HttpGet("uploads/download/{peerDocSysId:int}")]
        public async Task<IActionResult> Download(int peerDocSysId)
        {
            var doc = await _context.PeerDocs.AsNoTracking().FirstOrDefaultAsync(d => d.PeerDocSysId == peerDocSysId);
            if (doc == null || doc.Active != true) return NotFound();

            if (!System.IO.File.Exists(doc.DocPath))
                return NotFound(new { message = "File missing on server." });

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(doc.DocPath, out var contentType))
                contentType = "application/octet-stream";

            var fileName = Path.GetFileName(doc.DocPath);
            var bytes = await System.IO.File.ReadAllBytesAsync(doc.DocPath);

            return File(bytes, contentType, fileName);
        }
        [HttpDelete("uploads/{userId:guid}/{peerDocSysId:int}")]
        public async Task<IActionResult> DeleteUpload(Guid userId, int peerDocSysId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            var peer = await _context.PeerUsers.FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);
            if (peer == null) return NotFound();

            var doc = await _context.PeerDocs.FirstOrDefaultAsync(d => d.PeerDocSysId == peerDocSysId && d.PeerSysId == peer.PeerSysId);
            if (doc == null) return NotFound();

            doc.Active = false;
            doc.DateModify = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetUploads(userId);
        }

        // ==========================================================
        // Lookups needed for Step 1 dropdowns (Gender)
        // ==========================================================
        [HttpGet("lookups")]
        public async Task<IActionResult> GetLookups()
        {
            var genderRows = await _context.LkGenders
                .AsNoTracking()
                .OrderBy(g => g.SortKey ?? 9999)
                .ThenBy(g => g.Value)
                .Select(g => new { code = g.Code, value = g.Value })
                .ToListAsync();

            return Ok(new { genders = genderRows });
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic; 
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HIVTraining_Vue.Server.Services;
using System.Net;
using HIVTraining_Vue.Server.DTOs.PeerCertification;
using HIVTraining_Vue.Server.Models.Enums;

namespace HIVTraining_Vue.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PeerCertificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly BlobContainerClient _container;
        private readonly IEmailService _emailService;

        private bool _containerReady = false;

        private static readonly Dictionary<int, (int CourseSysId, string TrackCode)> PeerExamCourseMap = new()
{
    { 1010, (2001, "HIV") },
    { 1005, (2002, "HCV") },
    { 1003, (2003, "HR") },
    { 1007, (2003, "CJ") },
    { 5,    (2004, "PREP") }
};

        private async Task EnsureContainerAsync()
        {
            if (_containerReady) return;

            await _container.CreateIfNotExistsAsync(
                publicAccessType: PublicAccessType.None,
                metadata: null,
                encryptionScopeOptions: null,
                cancellationToken: default
            );

            _containerReady = true;
        }

        private static string GetReviewStatusText(int status)
        {
            return status switch
            {
                (int)EduCreditReviewStatus.Approved => "Approved",
                (int)EduCreditReviewStatus.Rejected => "Rejected",
                _ => "Pending"
            };
        }

        private static bool IsValidReviewStatus(int status)
        {
            return Enum.IsDefined(typeof(EduCreditReviewStatus), status);
        }

        private static string BuildDisplayFileName(
            string requestedName,
            string existingBlobPath)
        {
            var existingExtension = Path.GetExtension(existingBlobPath);

            var suppliedName = Path.GetFileName(requestedName)?.Trim();

            if (string.IsNullOrWhiteSpace(suppliedName))
                throw new ArgumentException("Document name is required.");

            suppliedName = SafeFileName(suppliedName);

            var suppliedExtension = Path.GetExtension(suppliedName);

            // Admin may enter only the name without the extension.
            if (string.IsNullOrWhiteSpace(suppliedExtension))
            {
                suppliedName += existingExtension;
            }
            else if (!string.Equals(
                         suppliedExtension,
                         existingExtension,
                         StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"The document extension must remain {existingExtension}.");
            }

            return suppliedName;
        }

        public PeerCertificationController(
    ApplicationDbContext context,
    IWebHostEnvironment env,
    IConfiguration config,
    IEmailService emailService)
        {
            _context = context;
            _env = env;
            _emailService = emailService;

            var cs = config["Storage:ConnectionString"];
            var containerName = config["Storage:ContainerName"] ?? "peer-cert";

            var serviceClient = new BlobServiceClient(cs);
            _container = serviceClient.GetBlobContainerClient(containerName);

            QuestPDF.Settings.License = LicenseType.Community;
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

        private static string GuessContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (provider.TryGetContentType(fileName, out var ct)) return ct;
            return "application/octet-stream";
        }

        private static string SafeBlobSegment(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "file";
            // keep it simple: avoid weird chars
            foreach (var c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            s = s.Replace("..", "_").Replace("/", "_").Replace("\\", "_");
            return s;
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
    required = (
    x.PeerDocId == 2 ||
    x.PeerDocId == 3 ||
    x.PeerDocId == 7 ||
    x.PeerDocId == 8
),
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
                    required = (id == 2 || id == 3 || id == 7 || id == 8),
                    active = true
                };
            }).ToList();

            return Ok(ordered);
        }

        private static string BoolText(bool? value)
        {
            if (!value.HasValue) return "—";
            return value.Value ? "Yes" : "No";
        }

        private static string DateText(DateTime? value)
        {
            return value?.ToString("MM/dd/yyyy") ?? "—";
        }

        private static string SafeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();
        }

        private async Task<string> GetLookupValueAsync<T>(
            IQueryable<T> query,
            Func<T, object?> codeSelector,
            Func<T, string?> valueSelector,
            object? code)
            where T : class
        {
            if (code == null) return "—";

            var items = await query.AsNoTracking().ToListAsync();
            var match = items.FirstOrDefault(x => string.Equals(
                Convert.ToString(codeSelector(x)),
                Convert.ToString(code),
                StringComparison.OrdinalIgnoreCase));

            return match == null ? Convert.ToString(code) ?? "—" : SafeText(valueSelector(match));
        }

        private void AddKeyValueTable(IContainer container, string title, List<(string Label, string Value)> rows)
        {
            container.Column(col =>
            {
                col.Item().PaddingBottom(8).Text(title).FontSize(15).Bold().FontColor("#1F1630");

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(180);
                        columns.RelativeColumn();
                    });

                    foreach (var row in rows)
                    {
                        table.Cell().Element(CellStyleHeader).Text(row.Label).FontSize(10).SemiBold();
                        table.Cell().Element(CellStyleValue).Text(row.Value).FontSize(10);
                    }
                });
            });

            static IContainer CellStyleHeader(IContainer container) =>
                container
                    .Border(1)
                    .BorderColor("#D9E0EA")
                    .Background("#F6F8FB")
                    .PaddingVertical(8)
                    .PaddingHorizontal(10);

            static IContainer CellStyleValue(IContainer container) =>
                container
                    .Border(1)
                    .BorderColor("#D9E0EA")
                    .PaddingVertical(8)
                    .PaddingHorizontal(10);
        }

        private void AddLongTextBlock(IContainer container, string title, string value)
        {
            container.Column(col =>
            {
                col.Item().PaddingBottom(4).Text(title).FontSize(11).SemiBold().FontColor("#344054");
                col.Item()
                    .Border(1)
                    .BorderColor("#D9E0EA")
                    .Background("#FFFFFF")
                    .Padding(10)
                    .Text(SafeText(value))
                    .FontSize(10)
                    .LineHeight(1.4f);
            });
        }
        [HttpGet("admin/manage-peer-detail/{userId:guid}/download-pdf")]
        public async Task<IActionResult> DownloadPeerApplicationPdf(Guid userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var peer = await _context.PeerUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);

            if (peer == null)
                return NotFound(new { message = "Peer application not found." });

            var aspUser = await _context.Set<ApplicationUser>()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Email == user.Email);

            var lastCourseAttendedDate = await _context.UserCourses
                .AsNoTracking()
                .Where(x => x.UserSysId == user.UserSysId && x.Attended == true)
                .MaxAsync(x => (DateTime?)(x.DateStatusChanged ?? x.DateModified ?? x.DateEntered));

            var uploads = await (
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
                    d.PeerDocId,
                    DocTypeName = t.Name,
                    d.DocPath,
                    d.DateUpload,
                    d.Reviewed
                }
            ).ToListAsync();

            var requiredScormIds = PeerExamCourseMap
                .Select(x => x.Value.CourseSysId)
                .Distinct()
                .ToList();

            var examSessions = await _context.ScormAiccSessions
                .AsNoTracking()
                .Where(x => x.Userid == user.UserSysId && requiredScormIds.Contains(x.Scormid))
                .GroupBy(x => x.Scormid)
                .Select(g => g.OrderByDescending(x => x.Attempt)
                              .ThenByDescending(x => x.Timemodified)
                              .FirstOrDefault())
                .ToListAsync();

            var exams = examSessions.Select((s, index) => new
            {
                ExamName = $"Exam {index + 1}",
                Status = s?.Lessonstatus ?? s?.Scormstatus ?? "Not Started",
                Completed = s != null && (
                    string.Equals(s.Scormstatus, "completed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Lessonstatus, "completed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Lessonstatus, "passed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Lessonstatus, "failed", StringComparison.OrdinalIgnoreCase)
                ),
                LastAttemptDate = s?.Timemodified
            }).ToList();

            var certificationTracks = new List<string>();
            if (peer.CertHiv == true) certificationTracks.Add("HIV");
            if (peer.CertHcv == true) certificationTracks.Add("HCV");
            if (peer.CertHr == true) certificationTracks.Add("HR");
            if (peer.CertPrep == true) certificationTracks.Add("PrEP");
            if (peer.CertCriminalJustice == true) certificationTracks.Add("CJ");

            var genderText = await GetLookupValueAsync(
                _context.LkGenders,
                x => ((LkGender)(object)x).Code,
                x => ((LkGender)(object)x).Value,
                peer.Gender);

            var educationText = await GetLookupValueAsync(
                _context.LkEducations,
                x => ((LkEducation)(object)x).Code,
                x => ((LkEducation)(object)x).Value,
                user.Education);

            var ethnicityText = await GetLookupValueAsync(
                _context.LkEthnicities,
                x => ((LkEthnicity)(object)x).Code,
                x => ((LkEthnicity)(object)x).Value,
                user.Ethnicity);

            var raceText = await GetLookupValueAsync(
                _context.LkRaces,
                x => ((LkRace)(object)x).Code,
                x => ((LkRace)(object)x).Value,
                user.Race);

            string statusText =
                peer.Active == false ? "Archived" :
                peer.Approve == true ? "Approved" :
                peer.Disapprove == true ? "Disapproved" :
                peer.Active == true ? "Submitted" :
                "In Progress";

            var fileName = $"{SafeFileName($"{user.FirstName}_{user.LastName}_Application")}.pdf";

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(28);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor("#111827"));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Peer Certification Application")
                            .FontSize(20)
                            .Bold()
                            .FontColor("#1F1630");

                        col.Item().PaddingTop(4).Text($"{SafeText(user.FirstName)} {SafeText(user.LastName)}")
                            .FontSize(13)
                            .SemiBold()
                            .FontColor("#4F2D6F");

                        col.Item().PaddingTop(2).Text($"Generated On: {DateTime.Now:MM/dd/yyyy hh:mm tt}")
                            .FontSize(9)
                            .FontColor("#667085");

                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#D9E0EA");
                    });

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(18);

                        // 1. Certification Review
                        col.Item().Element(c => AddKeyValueTable(c, "1. Certification Review", new List<(string, string)>
                {
                    ("Status", statusText),
                    ("Application Number", peer.ApplicantNumber?.ToString() ?? "—"),
                    ("Approve", BoolText(peer.Approve)),
                    ("Disapprove", BoolText(peer.Disapprove)),
                    ("Archived", peer.Active == false ? "Yes" : "No"),
                    ("Last Login", DateText(aspUser?.LastLoginDate)),
                    ("Last Course Attended", DateText(lastCourseAttendedDate)),
                    ("Approved Date", DateText(peer.ApprovedDt)),
                    ("Disapproved Date", DateText(peer.DisapprovedDt)),
                    ("Certification Tracks", certificationTracks.Any() ? string.Join(", ", certificationTracks) : "—"),
                    ("HIV Cert Date", DateText(peer.CertHivdate)),
                    ("HCV Cert Date", DateText(peer.CertHcvdate)),
                    ("HR Cert Date", DateText(peer.CertHrdate)),
                    ("PrEP Cert Date", DateText(peer.CertPrepDate)),
                    ("CJ Cert Date", DateText(peer.CertCriminalJusticeDate)),
                    ("Disapproval Reason", SafeText(peer.ReasonDisapprv))
                }));

                        // 2. Applicant Information
                        col.Item().Element(c => AddKeyValueTable(c, "2. Applicant Information", new List<(string, string)>
                {
                    ("First Name", SafeText(user.FirstName)),
                    ("Middle Initial", SafeText(user.Mi)),
                    ("Last Name", SafeText(user.LastName)),
                    ("Email", SafeText(user.Email)),
                    ("Alt Email", SafeText(user.AltEmail)),
                    ("Phone", SafeText(user.Phone)),
                    ("Alt Phone", SafeText(user.AltPhone)),
                    ("Cell Phone", SafeText(user.CellPhone)),
                    ("Work Phone", SafeText(user.WorkPhone)),
                    ("Work Phone Ext", SafeText(user.WorkPhoneExt)),
                    ("Primary Can Text", BoolText(user.PrimaryCanText)),
                    ("Alt Can Text", BoolText(user.AltCanText)),
                    ("Address", SafeText(user.Address)),
                    ("City", SafeText(user.City)),
                    ("State", SafeText(user.State)),
                    ("Zip", SafeText(user.Zip)),
                    ("Country", SafeText(user.Country)),
                    ("Title", SafeText(user.Title)),
                    ("Organization", SafeText(user.Organization)),
                    ("DOB", DateText(peer.Dob)),
                    ("Gender", genderText),
                    ("Agency Affiliation", SafeText(peer.AgencyAffilation)),
                    ("Education", educationText),
                    ("Ethnicity", ethnicityText),
                    ("Race", raceText),
                    ("Occupation", user.Occupation?.ToString() ?? "—"),
                    ("Years Current Occupation", user.YearsCurrentOccupation?.ToString() ?? "—"),
                    ("ADA Need", BoolText(user.Adaneed)),
                    ("ADA Details", SafeText(user.Adadetails))
                }));

                        // 3. Lived Experience
                        col.Item().Column(section =>
                        {
                            section.Spacing(8);
                            section.Item().Text("3. Lived Experience").FontSize(15).Bold().FontColor("#1F1630");
                            section.Item().Element(c => AddLongTextBlock(c, "Commitment to Wellness", peer.ExperienceCommitment));
                            section.Item().Element(c => AddLongTextBlock(c, "Challenges", peer.ExperienceChallenges));
                            section.Item().Element(c => AddLongTextBlock(c, "Why Serve as Peer Worker", peer.ExperienceWhy));
                            section.Item().Element(c => AddKeyValueTable(c, "", new List<(string, string)>
                    {
                        ("Self Care", BoolText(peer.SelfCare))
                    }));
                        });

                        // 4. Required Courses
                        col.Item().Column(section =>
                        {
                            section.Spacing(8);
                            section.Item().Text("4. Required Courses").FontSize(15).Bold().FontColor("#1F1630");
                            section.Item().Element(c => AddKeyValueTable(c, "", new List<(string, string)>
                    {
                        ("Required Courses Completed", BoolText(peer.RequiredCourses))
                    }));

                            if (exams.Any())
                            {
                                section.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1f);
                                        columns.RelativeColumn(1.3f);
                                    });

                                    void HeaderCell(string text) =>
                                        table.Cell().Border(1).BorderColor("#D9E0EA").Background("#F6F8FB").Padding(8)
                                            .Text(text).FontSize(10).SemiBold();

                                    void BodyCell(string text) =>
                                        table.Cell().Border(1).BorderColor("#D9E0EA").Padding(8)
                                            .Text(text).FontSize(10);

                                    HeaderCell("Exam");
                                    HeaderCell("Status");
                                    HeaderCell("Completed");
                                    HeaderCell("Last Attempt");

                                    foreach (var exam in exams)
                                    {
                                        BodyCell(exam.ExamName);
                                        BodyCell(SafeText(exam.Status));
                                        BodyCell(exam.Completed ? "Yes" : "No");
                                        BodyCell(DateText(exam.LastAttemptDate));
                                    }
                                });
                            }
                        });

                        // 5. Supervisor Information
                        col.Item().Element(c => AddKeyValueTable(c, "5. Supervisor Information", new List<(string, string)>
                {
                    ("Supervisor First Name", SafeText(peer.SupvrFirstName)),
                    ("Supervisor Last Name", SafeText(peer.SupvrLastName)),
                    ("Supervisor Org", SafeText(peer.SupvrOrgName)),
                    ("Supervisor Address 1", SafeText(peer.SupvrContAddr1)),
                    ("Supervisor Address 2", SafeText(peer.SupvrContAddr2)),
                    ("Supervisor City", SafeText(peer.SupvrContCity)),
                    ("Supervisor State", SafeText(peer.SupvrContState)),
                    ("Supervisor Zip", SafeText(peer.SupvrContZip)),
                    ("Supervisor Phone", SafeText(peer.SupvrContPhone)),
                    ("Supervisor Email", SafeText(peer.SupvrContEmail)),
                    ("Completed Practicum", BoolText(peer.ComplPracticum)),
                    ("500 Hours Minimum", BoolText(peer.ComplPracticumMin)),
                    ("Practicum Begin Date", DateText(peer.PracticumBdate)),
                    ("Practicum End Date", DateText(peer.PracticumEdate))
                }));

                        // 6. Documents
                        col.Item().Column(section =>
                        {
                            section.Spacing(8);
                            section.Item().Text("6. Documents").FontSize(15).Bold().FontColor("#1F1630");

                            if (!uploads.Any())
                            {
                                section.Item().Text("No documents uploaded.").FontSize(10).FontColor("#667085");
                            }
                            else
                            {
                                section.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.6f);
                                        columns.RelativeColumn(2.5f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(0.9f);
                                    });

                                    void HeaderCell(string text) =>
                                        table.Cell().Border(1).BorderColor("#D9E0EA").Background("#F6F8FB").Padding(8)
                                            .Text(text).FontSize(10).SemiBold();

                                    void BodyCell(string text) =>
                                        table.Cell().Border(1).BorderColor("#D9E0EA").Padding(8)
                                            .Text(text).FontSize(9);

                                    HeaderCell("Document Type");
                                    HeaderCell("File Name");
                                    HeaderCell("Uploaded");
                                    HeaderCell("Reviewed");

                                    foreach (var doc in uploads)
                                    {
                                        BodyCell(SafeText(doc.DocTypeName));
                                        BodyCell(SafeText(GetFileNameFromDocPath(doc.DocPath)));
                                        BodyCell(DateText(doc.DateUpload));
                                        BodyCell(doc.Reviewed == true ? "Yes" : "No");
                                    }
                                });
                            }
                        });

                        // 7. Admin Notes
                        col.Item().Column(section =>
                        {
                            section.Spacing(8);
                            section.Item().Text("7. Admin Notes").FontSize(15).Bold().FontColor("#1F1630");
                            section.Item()
                                .Border(1)
                                .BorderColor("#D9E0EA")
                                .Padding(10)
                                .Text(SafeText(peer.Notes))
                                .FontSize(10);
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Peer Certification Application | Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", fileName);
        }


        private static void PdfSection(IContainer container, string title, Action<ColumnDescriptor> content)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(12)
                .Column(column =>
                {
                    column.Item()
                        .Background(Colors.Purple.Lighten5)
                        .Padding(8)
                        .Text(title)
                        .Bold()
                        .FontSize(12)
                        .FontColor(Colors.Purple.Darken2);

                    column.Item().PaddingTop(8).Column(content);
                });
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

        [HttpGet("exams")]
        public async Task<IActionResult> GetPeerExams()
        {
            var titles = new[]
            {
        "HIV Peer Certification Exam",
        "HCV Peer Certification Exam",
        "Harm Reduction Peer Certification Exam",
        "PrEP Peer Certification Exam"
    };

            var rows = await _context.Subjects
                .AsNoTracking()
                .Where(s => s.Active
                    && s.IsOnlineTraining
                    && s.VideoUrl != null
                    && titles.Contains(s.CourseTitle))
                .Select(s => new
                {
                    subjectSysId = s.SubjectSysId,
                    title = s.CourseTitle,
                    videoUrl = s.VideoUrl
                })
                .ToListAsync();

            return Ok(rows);
        }

        [HttpGet("exam-courses/{userId:guid}")]
        public async Task<IActionResult> GetExamCourses(Guid userId, [FromQuery] string subjectIds)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var ids = (subjectIds ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x.Trim(), out var n) ? n : 0)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (!ids.Any())
                return Ok(Array.Empty<object>());

            var subjects = await _context.Subjects
                .AsNoTracking()
                .Where(s => ids.Contains(s.SubjectSysId) && s.Active)
                .Select(s => new
                {
                    s.SubjectSysId,
                    s.CourseTitle,
                    s.Description,
                    s.VideoUrl
                })
                .ToListAsync();

            var result = new List<object>();

            foreach (var s in subjects.OrderBy(x => ids.IndexOf(x.SubjectSysId)))
            {
                var mapped = PeerExamCourseMap.ContainsKey(s.SubjectSysId)
     ? PeerExamCourseMap[s.SubjectSysId]
     : (CourseSysId: 0, TrackCode: "");

                var mappedCourseSysId = mapped.CourseSysId;
                var trackCode = mapped.TrackCode;

                var latestSession = await _context.ScormAiccSessions
                    .AsNoTracking()
                    .Where(x => x.Userid == user.UserSysId && x.Scormid == mappedCourseSysId)
                    .OrderByDescending(x => x.Attempt)
                    .ThenByDescending(x => x.Timemodified)
                    .FirstOrDefaultAsync();

                bool completed = false;
                int percent = 0;

                if (latestSession != null)
                {
                    completed =
                        string.Equals(latestSession.Scormstatus, "completed", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(latestSession.Lessonstatus, "completed", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(latestSession.Lessonstatus, "passed", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(latestSession.Lessonstatus, "failed", StringComparison.OrdinalIgnoreCase);

                    if (completed)
                    {
                        percent = 100;
                    }
                    else
                    {
                        var progressTrack = await _context.ScormScoesTracks
                            .AsNoTracking()
                            .Where(t =>
                                t.Userid == user.UserSysId &&
                                t.Scormid == mappedCourseSysId &&
                                t.Attempt == latestSession.Attempt &&
                                t.Element == "cmi.progress_measure")
                            .OrderByDescending(t => t.Timemodified)
                            .FirstOrDefaultAsync();

                        if (progressTrack != null &&
                            double.TryParse(progressTrack.Value, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out var progress))
                        {
                            percent = (int)Math.Round(progress * 100.0);
                        }
                        else
                        {
                            var locationTrack = await _context.ScormScoesTracks
                                .AsNoTracking()
                                .Where(t =>
                                    t.Userid == user.UserSysId &&
                                    t.Scormid == mappedCourseSysId &&
                                    t.Attempt == latestSession.Attempt &&
                                    t.Element == "cmi.core.lesson_location")
                                .OrderByDescending(t => t.Timemodified)
                                .FirstOrDefaultAsync();

                            if (locationTrack != null &&
                                int.TryParse(locationTrack.Value, out var bookmarkPercent))
                            {
                                percent = Math.Max(0, Math.Min(100, bookmarkPercent));
                            }
                        }
                    }
                }

                result.Add(new
                {
                    subjectSysId = s.SubjectSysId,
                    courseSysId = mappedCourseSysId,
                    courseTitle = s.CourseTitle,
                    description = s.Description,
                    videoUrl = s.VideoUrl,
                    scormId = mappedCourseSysId,
                    scoId = "",
                    trackCode,
                    completed,
                    percent
                });
            }

            return Ok(result);
        }
        [HttpPost("register-exam-course")]
        public async Task<IActionResult> RegisterExamCourse([FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("userId", out var userIdEl))
                    return BadRequest(new { message = "Missing userId" });

                if (!Guid.TryParse(userIdEl.GetString(), out var userGuid))
                    return BadRequest(new { message = "Invalid userId" });

                if (!body.TryGetProperty("courseSysId", out var courseEl))
                    return BadRequest(new { message = "Missing courseSysId" });

                var courseSysId = courseEl.GetInt32();
                if (courseSysId <= 0)
                    return BadRequest(new { message = "Invalid courseSysId" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userGuid);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseSysId == courseSysId);
                if (course == null)
                    return NotFound(new { message = "Course not found" });

                var existing = await _context.UserCourses
                    .FirstOrDefaultAsync(uc =>
                        uc.UserSysId == user.UserSysId &&
                        uc.CourseSysId == courseSysId &&
                        uc.Status == 1);

                if (existing != null)
                {
                    return Ok(new
                    {
                        message = "Already registered.",
                        alreadyRegistered = true
                    });
                }

                var userCourse = new UserCourse
                {
                    UserSysId = user.UserSysId,
                    CourseSysId = courseSysId,
                    Status = 1,
                    DateEntered = DateTime.UtcNow,
                    DateStatusChanged = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                    Token = Guid.NewGuid(),
                    IsWaitlisted = false,
                    WaitlistNumber = null
                };

                _context.UserCourses.Add(userCourse);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Exam course registered successfully.",
                    alreadyRegistered = false
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to register exam course.",
                    detail = ex.Message
                });
            }
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

            var tracks = new List<string>();

            if (peer?.CertHiv == true) tracks.Add("HIV");
            if (peer?.CertHcv == true) tracks.Add("HCV");
            if (peer?.CertHr == true) tracks.Add("HR");
            if (peer?.CertPrep == true) tracks.Add("PREP");
            if (peer?.CertCriminalJustice == true) tracks.Add("CJ");

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

                CertificationTrack = tracks,

                ExperienceCommitment = peer?.ExperienceCommitment,
                ExperienceChallenges = peer?.ExperienceChallenges,
                ExperienceWhy = peer?.ExperienceWhy,
                SelfCare = peer?.SelfCare,
                ApplicationPercentage = peer?.ApplicationPercentage ?? 0,

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

            static List<string> GetStringList(JsonElement e, string name)
            {
                var result = new List<string>();

                if (!e.TryGetProperty(name, out var p))
                    return result;

                if (p.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in p.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            var s = item.GetString();
                            if (!string.IsNullOrWhiteSpace(s))
                                result.Add(s.Trim().ToUpper());
                        }
                    }
                }
                else if (p.ValueKind == JsonValueKind.String)
                {
                    var s = p.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                        result.Add(s.Trim().ToUpper());
                }

                return result.Distinct().ToList();
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
                    Active = null,
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

            if (HasProp(body, "CertificationTrack"))
            {
                var trackVals = GetStringList(body, "CertificationTrack");
                var now = DateTime.UtcNow;

                if (trackVals.Contains("HIV") && trackVals.Contains("PREP"))
                {
                    return BadRequest(new { message = "HIV and PrEP certification tracks cannot be selected together." });
                }

                // HIV
                if (trackVals.Contains("HIV"))
                {
                    peer.CertHiv = true;
                    if (peer.CertHivdate == null)
                        peer.CertHivdate = now;
                }
                else
                {
                    peer.CertHiv = false;
                    peer.CertHivdate = null;
                }

                // HCV
                if (trackVals.Contains("HCV"))
                {
                    peer.CertHcv = true;
                    if (peer.CertHcvdate == null)
                        peer.CertHcvdate = now;
                }
                else
                {
                    peer.CertHcv = false;
                    peer.CertHcvdate = null;
                }

                // Harm Reduction
                if (trackVals.Contains("HR"))
                {
                    peer.CertHr = true;
                    if (peer.CertHrdate == null)
                        peer.CertHrdate = now;
                }
                else
                {
                    peer.CertHr = false;
                    peer.CertHrdate = null;
                }

                // PrEP
                if (trackVals.Contains("PREP"))
                {
                    peer.CertPrep = true;
                    if (peer.CertPrepDate == null)
                        peer.CertPrepDate = now;
                }
                else
                {
                    peer.CertPrep = false;
                    peer.CertPrepDate = null;
                }

                // Criminal Justice
                if (trackVals.Contains("CJ"))
                {
                    peer.CertCriminalJustice = true;
                    if (peer.CertCriminalJusticeDate == null)
                        peer.CertCriminalJusticeDate = now;
                }
                else
                {
                    peer.CertCriminalJustice = false;
                    peer.CertCriminalJusticeDate = null;
                }
            }


            if (HasProp(body, "ApplicationPercentage"))
            {
                var pct = GetInt(body, "ApplicationPercentage");
                if (pct.HasValue)
                {
                    var safePct = Math.Max(0, Math.Min(99, pct.Value));
                    peer.ApplicationPercentage = safePct;
                }
            }
            peer.DateModify = DateTime.UtcNow;
            //peer.Active = true;

            await _context.SaveChangesAsync();

            return await GetApplicantInfo(userId);
        }
        [HttpGet("track/{userId:guid}")]
        public async Task<IActionResult> TrackApplications(Guid userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found.");

            var peers = await _context.PeerUsers
                .AsNoTracking()
                .Where(p => p.UserSysId == user.UserSysId)
                .OrderByDescending(p => p.DateCreate)
                .ToListAsync();

            if (!peers.Any())
                return Ok(new List<object>());

            var requiredDocIds = new[] { 2, 3, 7, 8 };
            var requiredScormIds = PeerExamCourseMap
    .Select(x => x.Value.CourseSysId)
    .Distinct()
    .ToList();

            var results = new List<object>();

            foreach (var peer in peers)
            {
                var certificationTrack =
                    peer.CertHiv == true ? "HIV" :
                    peer.CertHcv == true ? "HCV" :
                    peer.CertHr == true ? "Harm Reduction" :
                    peer.CertPrep == true ? "PrEP" :
                    peer.CertCriminalJustice == true ? "Criminal Justice" :
                    "";

                var uploadedRequiredDocs = await _context.PeerDocs
                    .AsNoTracking()
                    .Where(d => d.PeerSysId == peer.PeerSysId && d.Active == true && requiredDocIds.Contains(d.PeerDocId))
                    .Select(d => d.PeerDocId)
                    .Distinct()
                    .CountAsync();

                var sessions = await _context.ScormAiccSessions
                    .AsNoTracking()
                    .Where(x => x.Userid == user.UserSysId && requiredScormIds.Contains(x.Scormid))
                    .GroupBy(x => x.Scormid)
                    .Select(g => g
                        .OrderByDescending(x => x.Attempt)
                        .ThenByDescending(x => x.Timemodified)
                        .FirstOrDefault())
                    .ToListAsync();

                var completedExamCount = sessions
                    .Where(s => s != null &&
                        (
                            string.Equals(s.Scormstatus, "completed", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s.Lessonstatus, "completed", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s.Lessonstatus, "passed", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s.Lessonstatus, "failed", StringComparison.OrdinalIgnoreCase)
                        ))
                    .Select(s => s.Scormid)
                    .Distinct()
                    .Count();

                string appStatus;
                if (peer.Approve == true)
                    appStatus = "Approved";
                else if (peer.Disapprove == true)
                    appStatus = "Disapproved";
                else if (peer.Active == false)
                    appStatus = "Archived";
                else if (peer.Active == true && (peer.ApplicationPercentage ?? 0) == 100)
                    appStatus = "Submitted";
                else
                    appStatus = "In Progress";

                results.Add(new
                {
                    peerSysId = peer.PeerSysId,
                    certificationTrack,
                    applicationStatus = appStatus,
                    active = peer.Active == true,
                    approved = peer.Approve == true,
                    submittedOn = peer.DateCreate,
                    lastUpdated = peer.DateModify,
                    requiredCoursesConfirmed = peer.RequiredCourses == true,
                    selfCareCompleted = peer.SelfCare == true,
                    practicumCompleted = peer.ComplPracticum == true,
                    practicumMin500 = peer.ComplPracticumMin == true,
                    uploadedRequiredDocs,
                    totalRequiredDocs = requiredDocIds.Length,
                    requiredUploadsComplete = uploadedRequiredDocs == requiredDocIds.Length,
                    completedExamCount,
                    totalExamCount = requiredScormIds.Count
                });
            }

            return Ok(results);
        }

        [HttpGet("admin/manage-peer")]
        public async Task<IActionResult> GetManagePeerList(
    [FromQuery] string view = "all",
    [FromQuery] string? search = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize switch
            {
                10 => 10,
                20 => 20,
                50 => 50,
                100 => 100,
                _ => 10
            };

            var query =
                from p in _context.PeerUsers.AsNoTracking()
                join u in _context.Users.AsNoTracking()
                    on p.UserSysId equals u.UserSysId
                join au in _context.Set<ApplicationUser>().AsNoTracking()
                on u.Email equals au.Email into aspJoin
                from asp in aspJoin.DefaultIfEmpty()
                select new
                {
                    Peer = p,
                    User = u,
                    AspUser = asp
                };

            view = (view ?? "all").Trim().ToLower();

            switch (view)
            {
                case "inprogress":
                    query = query.Where(x =>
                        x.Peer.Approve != true &&
                        x.Peer.Disapprove != true &&
                        x.Peer.Closed != true &&
                        x.Peer.Lapsed != true &&
                        x.Peer.Active != false &&
                        (
                            x.Peer.Active != true ||
                            (x.Peer.ApplicationPercentage ?? 0) < 100
                        ));
                    break;

                case "submitted":
                    query = query.Where(x =>
                        x.Peer.Approve != true &&
                        x.Peer.Disapprove != true &&
                        x.Peer.Closed != true &&
                        x.Peer.Lapsed != true &&
                        x.Peer.Active == true &&
                        (x.Peer.ApplicationPercentage ?? 0) == 100);
                    break;

                case "approved":
                    query = query.Where(x =>
                        x.Peer.Approve == true &&
                        x.Peer.Disapprove != true &&
                        x.Peer.Closed != true &&
                        x.Peer.Lapsed != true &&
                        x.Peer.Active == true);
                    break;

                case "disapproved":
                    query = query.Where(x =>
                        x.Peer.Disapprove == true &&
                        x.Peer.Approve != true &&
                        x.Peer.Closed != true &&
                        x.Peer.Lapsed != true &&
                        x.Peer.Active == true);
                    break;

                case "archived":
                    query = query.Where(x =>
                        x.Peer.Approve != true &&
                        x.Peer.Disapprove != true &&
                        x.Peer.Closed != true &&
                        x.Peer.Lapsed != true &&
                        x.Peer.Active == false);
                    break;

                case "closed":
                    query = query.Where(x =>
                        x.Peer.Closed == true &&
                        x.Peer.Approve != true &&
                        x.Peer.Disapprove != true &&
                        x.Peer.Lapsed != true);
                    break;

                case "lapsed":
                    query = query.Where(x =>
                        x.Peer.Lapsed == true &&
                        x.Peer.Approve != true &&
                        x.Peer.Disapprove != true &&
                        x.Peer.Closed != true);
                    break;

                case "all":
                default:
                    break;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();

                query = query.Where(x =>
                    ((x.User.FirstName ?? "").ToLower().Contains(term)) ||
                    ((x.User.LastName ?? "").ToLower().Contains(term)) ||
                    (((x.User.FirstName ?? "") + " " + (x.User.LastName ?? "")).ToLower().Contains(term)));
            }

            var totalRecords = await query.CountAsync();

            var pagedRows = await query
    .OrderByDescending(x => x.Peer.DateCreate)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(x => new
    {
        peerSysId = x.Peer.PeerSysId,
        userSysId = x.User.UserSysId,
        userId = x.User.UserId,

        firstName = x.User.FirstName,
        lastName = x.User.LastName,
        fullName = ((x.User.FirstName ?? "") + " " + (x.User.LastName ?? "")).Trim(),

        certHiv = x.Peer.CertHiv,
        certHcv = x.Peer.CertHcv,
        certHr = x.Peer.CertHr,
        certPrep = x.Peer.CertPrep,
        certCriminalJustice = x.Peer.CertCriminalJustice,

        approve = x.Peer.Approve,
        disapprove = x.Peer.Disapprove,
        closed = x.Peer.Closed,
        lapsed = x.Peer.Lapsed,
        active = x.Peer.Active,
        submittedOn = x.Peer.DateCreate,
        lastUpdated = x.Peer.DateModify,
        approvedDt = x.Peer.ApprovedDt,
        disapprovedDt = x.Peer.DisapprovedDt,

        applicationPercentage = x.Peer.ApplicationPercentage,

        lastLoginDate = x.AspUser != null ? x.AspUser.LastLoginDate : null,

        applicationStatus =
    x.Peer.Closed == true ? "Closed" :
    x.Peer.Lapsed == true ? "Lapsed" :
    x.Peer.Approve == true ? "Approved" :
    x.Peer.Disapprove == true ? "Disapproved" :
    (
        x.Peer.Active == false &&
        x.Peer.Closed != true &&
        x.Peer.Lapsed != true
    ) ? "Archived" :
    (
        x.Peer.Active == true &&
        (x.Peer.ApplicationPercentage ?? 0) == 100
    ) ? "Submitted" :
    "In Progress"
    })
    .ToListAsync();

            var userSysIds = pagedRows.Select(x => x.userSysId).Distinct().ToList();

            var lastAttendedMap = await _context.UserCourses
                .AsNoTracking()
                .Where(uc =>
                    userSysIds.Contains(uc.UserSysId) &&
                    uc.Attended == true)
                .GroupBy(uc => uc.UserSysId)
                .Select(g => new
                {
                    userSysId = g.Key,
                    lastCourseAttendedDate = g.Max(x => x.DateStatusChanged ?? x.DateModified ?? x.DateEntered)
                })
                .ToDictionaryAsync(x => x.userSysId, x => x.lastCourseAttendedDate);

            var rows = pagedRows.Select(x => new
            {
                x.peerSysId,
                x.userSysId,
                x.userId,
                x.firstName,
                x.lastName,
                x.fullName,

                certificationTrack = string.Join(", ", new[]
    {
        x.certHiv == true ? "HIV" : null,
        x.certHcv == true ? "HCV" : null,
        x.certHr == true ? "HR" : null,
        x.certPrep == true ? "PrEP" : null,
        x.certCriminalJustice == true ? "CJ" : null
    }.Where(t => !string.IsNullOrWhiteSpace(t))),

                x.approve,
                x.disapprove,
                x.closed,
                x.lapsed,
                x.active,
                x.submittedOn,
                x.lastUpdated,
                x.approvedDt,
                x.disapprovedDt,
                x.lastLoginDate,
                lastCourseAttendedDate = lastAttendedMap.ContainsKey(x.userSysId)
        ? lastAttendedMap[x.userSysId]
        : null,
                applicationPercentage = x.applicationPercentage ?? 0,
                x.applicationStatus
            }).ToList();

            return Ok(new
            {
                totalRecords,
                page,
                pageSize,
                items = rows
            });
        }

        [HttpGet("continuing-education-eligibility/{userId:guid}")]
        public async Task<IActionResult> GetContinuingEducationEligibility(Guid userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    eligible = false,
                    message = "User not found."
                });
            }

            var hasApprovedPeer = await _context.PeerUsers
    .AsNoTracking()
    .AnyAsync(p => p.UserSysId == user.UserSysId && p.Approve == true && p.Active == true);

            if (!hasApprovedPeer)
            {
                return Ok(new
                {
                    eligible = false,
                    message = "Continuing Education Credits are available only for approved peer-certified users."
                });
            }

            return Ok(new
            {
                eligible = true
            });


        }

        [HttpGet("admin/manage-edu-credits")]
        public async Task<IActionResult> GetManageEduCredits(
    [FromQuery] string? search = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            const int ceDocType = 9;

            page = Math.Max(page, 1);

            pageSize = pageSize switch
            {
                10 => 10,
                20 => 20,
                50 => 50,
                100 => 100,
                _ => 10
            };

            var query =
                from document in _context.PeerDocs.AsNoTracking()
                join peer in _context.PeerUsers.AsNoTracking()
                    on document.PeerSysId equals peer.PeerSysId
                join user in _context.Users.AsNoTracking()
                    on peer.UserSysId equals user.UserSysId
                where document.Active == true
                      && document.PeerDocId == ceDocType
                group document by new
                {
                    peer.PeerSysId,
                    user.UserId,
                    user.UserSysId,
                    user.FirstName,
                    user.LastName,
                    user.Email
                }
                into documentGroup
                select new
                {
                    peerSysId = documentGroup.Key.PeerSysId,
                    userId = documentGroup.Key.UserId,
                    userSysId = documentGroup.Key.UserSysId,
                    firstName = documentGroup.Key.FirstName,
                    lastName = documentGroup.Key.LastName,

                    fullName =
                        ((documentGroup.Key.FirstName ?? string.Empty) + " " +
                         (documentGroup.Key.LastName ?? string.Empty)).Trim(),

                    email = documentGroup.Key.Email,

                    documentCount = documentGroup.Count(),

                    totalCredits = documentGroup.Sum(
                        document => document.NoOfCredits ?? 0),

                    pendingCount = documentGroup.Count(document =>
     document.ReviewStatus == (int)EduCreditReviewStatus.Pending &&
     document.Reviewed == false),

                    approvedCount = documentGroup.Count(document =>
                        document.ReviewStatus == (int)EduCreditReviewStatus.Approved ||
                        (
                            document.ReviewStatus == (int)EduCreditReviewStatus.Pending &&
                            document.Reviewed == true
                        )),

                    rejectedCount = documentGroup.Count(document =>
                        document.ReviewStatus == (int)EduCreditReviewStatus.Rejected),

                    latestUploadDate = documentGroup.Max(
                        document => document.DateUpload)
                };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();

                query = query.Where(row =>
                    (row.firstName ?? string.Empty).ToLower().Contains(term) ||
                    (row.lastName ?? string.Empty).ToLower().Contains(term) ||
                    (
                        (row.firstName ?? string.Empty) + " " +
                        (row.lastName ?? string.Empty)
                    ).ToLower().Contains(term) ||
                    (row.email ?? string.Empty).ToLower().Contains(term));
            }

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderByDescending(row => row.latestUploadDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalRecords,
                page,
                pageSize,
                items
            });
        }
        [HttpGet("admin/manage-edu-credits/{peerSysId:int}/documents")]
        public async Task<ActionResult<List<EduCreditDocumentDto>>>
    GetEduCreditDocumentsByPeer(int peerSysId)
        {
            const int ceDocType = 9;

            var documents = await _context.PeerDocs
                .AsNoTracking()
                .Where(document =>
                    document.PeerSysId == peerSysId &&
                    document.Active == true &&
                    document.PeerDocId == ceDocType)
                .OrderByDescending(document => document.DateUpload)
                .Select(document => new
                {
                    document.PeerDocSysId,
                    document.PeerSysId,
                    document.DocPath,
                    document.DisplayFileName,
                    document.NoOfCredits,
                    document.DateUpload,
                    document.ReviewStatus,
                    document.AdminComments
                })
                .ToListAsync();

            var result = documents.Select(document =>
            {
                var fallbackFileName =
                    GetFileNameFromDocPath(document.DocPath);
                var effectiveReviewStatus = document.ReviewStatus;

                return new EduCreditDocumentDto
                {
                    PeerDocSysId = document.PeerDocSysId,
                    PeerSysId = document.PeerSysId,

                    FileName = string.IsNullOrWhiteSpace(document.DisplayFileName)
                        ? fallbackFileName
                        : document.DisplayFileName,

                    NoOfCredits = document.NoOfCredits,
                    DateUpload = document.DateUpload,

                    ReviewStatus = effectiveReviewStatus,
                    ReviewStatusText = GetReviewStatusText(effectiveReviewStatus),

                    AdminComments = document.AdminComments
                };
            }).ToList();

            return Ok(result);
        }

        [HttpPut("admin/manage-edu-credits/{peerDocSysId:int}")]
        public async Task<IActionResult> UpdateEduCreditDocument(
    int peerDocSysId,
    [FromBody] UpdateEduCreditDocumentDto request)
        {
            const int ceDocType = 9;

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (!IsValidReviewStatus(request.ReviewStatus))
            {
                return BadRequest(new
                {
                    message = "Review status must be Pending, Approved, or Rejected."
                });
            }

            if (request.NoOfCredits.HasValue &&
                request.NoOfCredits.Value < 0)
            {
                return BadRequest(new
                {
                    message = "The number of credits cannot be negative."
                });
            }

            if (request.ReviewStatus ==
                    (int)EduCreditReviewStatus.Rejected &&
                string.IsNullOrWhiteSpace(request.AdminComments))
            {
                return BadRequest(new
                {
                    message = "Admin comments are required when rejecting a document."
                });
            }

            var document = await _context.PeerDocs
                .FirstOrDefaultAsync(document =>
                    document.PeerDocSysId == peerDocSysId &&
                    document.PeerDocId == ceDocType &&
                    document.Active == true);

            if (document == null)
            {
                return NotFound(new
                {
                    message = "Continuing education document was not found."
                });
            }

            string displayFileName;

            try
            {
                displayFileName = BuildDisplayFileName(
                    request.FileName,
                    document.DocPath);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }

            document.DisplayFileName = displayFileName;
            document.NoOfCredits = request.NoOfCredits;
            document.ReviewStatus = request.ReviewStatus;

            document.AdminComments =
                string.IsNullOrWhiteSpace(request.AdminComments)
                    ? null
                    : request.AdminComments.Trim();

            document.DateModify = DateTime.UtcNow;

            // Maintain legacy Reviewed until the old field is removed.
            document.Reviewed =
                request.ReviewStatus ==
                (int)EduCreditReviewStatus.Approved;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Educational credit document updated successfully.",
                document = new EduCreditDocumentDto
                {
                    PeerDocSysId = document.PeerDocSysId,
                    PeerSysId = document.PeerSysId,
                    FileName = document.DisplayFileName,
                    NoOfCredits = document.NoOfCredits,
                    DateUpload = document.DateUpload,
                    ReviewStatus = document.ReviewStatus,
                    ReviewStatusText =
                        GetReviewStatusText(document.ReviewStatus),
                    AdminComments = document.AdminComments
                }
            });
        }
        [HttpPost("continuing-education/upload/{userId:guid}")]
        [RequestSizeLimit(MaxUploadBytes)]
        public async Task<IActionResult> UploadContinuingEducationDoc(
    Guid userId,
    [FromForm] IFormFile file,
    [FromForm] decimal? noOfCredits)
        {
            const int ceDocType = 9;

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (file.Length > MaxUploadBytes)
                return BadRequest("File too large.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExt.Contains(ext))
                return BadRequest("Invalid file type.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound("User not found.");

            var peer = await _context.PeerUsers
    .Where(p => p.UserSysId == user.UserSysId && p.Approve == true && p.Active == true)
    .OrderByDescending(p => p.ApprovedDt ?? p.DateModify ?? p.DateCreate)
    .FirstOrDefaultAsync();

            if (peer == null)
                return BadRequest("Only approved peer-certified users can upload CE documents.");

            await EnsureContainerAsync();

            var originalSafe = SafeBlobSegment(Path.GetFileName(file.FileName));
            var ext2 = Path.GetExtension(originalSafe).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext2)) ext2 = ext;

            var storedName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{ext2}";
            var blobName = $"peeruploads/{peer.PeerSysId}/{ceDocType}/{storedName}";

            var blob = _container.GetBlobClient(blobName);

            await using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? GuessContentType(originalSafe)
                        : file.ContentType
                });
            }

            var doc = new PeerDoc
            {
                PeerSysId = peer.PeerSysId,
                PeerDocId = ceDocType,
                DocType = ceDocType,

                DocPath = blobName,
                DisplayFileName = originalSafe,

                DateUpload = DateTime.UtcNow,
                Active = true,
                UploadBy = user.Email ?? user.UserSysId.ToString(),

                Reviewed = false,
                ReviewStatus = (int)EduCreditReviewStatus.Pending,
                AdminComments = null,

                NoOfCredits = noOfCredits
            };

            _context.PeerDocs.Add(doc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Continuing education document uploaded successfully." });
        }
        [HttpGet("continuing-education/{userId:guid}")]
        public async Task<IActionResult> GetContinuingEducationPage(Guid userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var approvedPeers = await _context.PeerUsers
                .AsNoTracking()
                .Where(p => p.UserSysId == user.UserSysId && p.Approve == true && p.Active == true)
                .OrderByDescending(p => p.ApprovedDt ?? p.DateModify ?? p.DateCreate)
                .ToListAsync();

            if (!approvedPeers.Any())
                return BadRequest(new { message = "Continuing Education Credits are available only for approved peer-certified users." });

            var certificationTracks = approvedPeers
                .SelectMany(peer =>
                {
                    var tracks = new List<object>();

                    if (peer.CertHiv == true)
                        tracks.Add(new { code = "HIV", certDate = peer.CertHivdate });

                    if (peer.CertHcv == true)
                        tracks.Add(new { code = "HCV", certDate = peer.CertHcvdate });

                    if (peer.CertHr == true)
                        tracks.Add(new { code = "HR", certDate = peer.CertHrdate });

                    if (peer.CertPrep == true)
                        tracks.Add(new { code = "PREP", certDate = peer.CertPrepDate });

                    if (peer.CertCriminalJustice == true)
                        tracks.Add(new { code = "CJ", certDate = peer.CertCriminalJusticeDate });

                    return tracks;
                })
                .GroupBy(x => ((string)x.GetType().GetProperty("code")!.GetValue(x)!).ToUpper())
                .Select(g => g
                    .OrderByDescending(x => (DateTime?)x.GetType().GetProperty("certDate")!.GetValue(x))
                    .First())
                .ToList();

            var approvedPeerIds = approvedPeers.Select(p => p.PeerSysId).ToList();

            var documentRows = await _context.PeerDocs
    .AsNoTracking()
    .Where(document =>
        approvedPeerIds.Contains(document.PeerSysId) &&
        document.Active == true &&
        document.PeerDocId == 9)
    .OrderByDescending(document => document.DateUpload)
    .Select(document => new
    {
        document.PeerDocSysId,
        document.PeerDocId,
        document.DocPath,
        document.DisplayFileName,
        document.NoOfCredits,
        document.DateUpload,
        document.ReviewStatus,
        document.Reviewed,
        document.AdminComments
    })
    .ToListAsync();

            var documents = documentRows.Select(document =>
            {
                var effectiveReviewStatus =
                    document.ReviewStatus == (int)EduCreditReviewStatus.Pending &&
                    document.Reviewed
                        ? (int)EduCreditReviewStatus.Approved
                        : document.ReviewStatus;

                return new
                {
                    peerDocSysId = document.PeerDocSysId,
                    peerDocId = document.PeerDocId,

                    fileName = string.IsNullOrWhiteSpace(document.DisplayFileName)
                        ? GetFileNameFromDocPath(document.DocPath)
                        : document.DisplayFileName,

                    noOfCredits = document.NoOfCredits,
                    dateUpload = document.DateUpload,

                    reviewStatus = effectiveReviewStatus,
                    reviewStatusText = GetReviewStatusText(effectiveReviewStatus),

                    adminComments = document.AdminComments
                };
            }).ToList();

            return Ok(new
            {
                certificationTracks,
                documents
            });
        }
        [HttpGet("admin/manage-peer-detail/{userId:guid}")]
        public async Task<IActionResult> GetManagePeerDetail(Guid userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var peer = await _context.PeerUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);

            if (peer == null)
                return NotFound(new { message = "Peer application not found." });

            var aspUser = await _context.Set<ApplicationUser>()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Email == user.Email);

            var lastCourseAttendedDate = await _context.UserCourses
                .AsNoTracking()
                .Where(x => x.UserSysId == user.UserSysId && x.Attended == true)
                .MaxAsync(x => (DateTime?)(x.DateStatusChanged ?? x.DateModified ?? x.DateEntered));

            var uploads = await (
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
                    d.PeerDocId,
                    docTypeName = t.Name,
                    d.DocPath,
                    d.DateUpload,
                    d.Reviewed
                }
            ).ToListAsync();

            var requiredScormIds = PeerExamCourseMap
                .Select(x => x.Value.CourseSysId)
                .Distinct()
                .ToList();

            var examSessions = await _context.ScormAiccSessions
                .AsNoTracking()
                .Where(x => x.Userid == user.UserSysId && requiredScormIds.Contains(x.Scormid))
                .GroupBy(x => x.Scormid)
                .Select(g => g.OrderByDescending(x => x.Attempt)
                              .ThenByDescending(x => x.Timemodified)
                              .FirstOrDefault())
                .ToListAsync();

            var exams = examSessions.Select(s => new
            {
                scormId = s?.Scormid,
                completed = s != null && (
                    string.Equals(s.Scormstatus, "completed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Lessonstatus, "completed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Lessonstatus, "passed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.Lessonstatus, "failed", StringComparison.OrdinalIgnoreCase)
                ),
                status = s?.Lessonstatus ?? s?.Scormstatus ?? "Not Started",
                lastAttemptDate = s?.Timemodified
            }).ToList();

            var certificationTrack = new List<object>();

            DateTime? certHivdate = peer.CertHivdate;
            DateTime? certHcvdate = peer.CertHcvdate;
            DateTime? certHrdate = peer.CertHrdate;
            DateTime? certPrepDate = peer.CertPrepDate;
            DateTime? certCriminalJusticeDate = peer.CertCriminalJusticeDate;

            if (peer.CertHiv == true)
                certificationTrack.Add(new { code = "HIV", certDate = certHivdate });

            if (peer.CertHcv == true)
                certificationTrack.Add(new { code = "HCV", certDate = certHcvdate });

            if (peer.CertHr == true)
                certificationTrack.Add(new { code = "HR", certDate = certHrdate });

            if (peer.CertPrep == true)
                certificationTrack.Add(new { code = "PREP", certDate = certPrepDate });

            if (peer.CertCriminalJustice == true)
                certificationTrack.Add(new { code = "CJ", certDate = certCriminalJusticeDate });


            Console.WriteLine($"PeerSysId: {peer.PeerSysId}");
            Console.WriteLine($"UserSysId: {peer.UserSysId}");
            Console.WriteLine($"CertHivdate: {peer.CertHivdate}");
            Console.WriteLine($"CertHcvdate: {peer.CertHcvdate}");
            Console.WriteLine($"CertHrdate: {peer.CertHrdate}");
            Console.WriteLine($"CertPrepDate: {peer.CertPrepDate}");
            Console.WriteLine($"CertCriminalJusticeDate: {peer.CertCriminalJusticeDate}");
            return Ok(new
            {
                userId = user.UserId,
                userSysId = user.UserSysId,
                peerSysId = peer.PeerSysId,

                fullName = ((user.FirstName ?? "") + " " + (user.LastName ?? "")).Trim(),
                firstName = user.FirstName,
                mi = user.Mi,
                lastName = user.LastName,
                email = user.Email,
                altEmail = user.AltEmail,
                phone = user.Phone,
                altPhone = user.AltPhone,
                cellPhone = user.CellPhone,
                workPhone = user.WorkPhone,
                workPhoneExt = user.WorkPhoneExt,
                address = user.Address,
                city = user.City,
                state = user.State,
                zip = user.Zip,
                country = user.Country,
                title = user.Title,
                organization = user.Organization,
                education = user.Education,
                ethnicity = user.Ethnicity,
                race = user.Race,
                occupation = user.Occupation,
                yearsCurrentOccupation = user.YearsCurrentOccupation,
                adaneed = user.Adaneed,
                adadetails = user.Adadetails,

                applicationPercentage = peer.ApplicationPercentage ?? 0,

                dob = peer.Dob,
                gender = peer.Gender,
                agencyAffilation = peer.AgencyAffilation,
                applicantNumber = peer.ApplicantNumber,

                approve = peer.Approve,
                disapprove = peer.Disapprove,
                active = peer.Active,
                closed = peer.Closed,
                lapsed = peer.Lapsed,
                approvedDt = peer.ApprovedDt,
                disapprovedDt = peer.DisapprovedDt,

                certHivdate = peer.CertHivdate,
                certHcvdate = peer.CertHcvdate,
                certHrdate = peer.CertHrdate,
                certPrepDate = peer.CertPrepDate,
                certCriminalJusticeDate = peer.CertCriminalJusticeDate,

                experienceCommitment = peer.ExperienceCommitment,
                experienceChallenges = peer.ExperienceChallenges,
                experienceWhy = peer.ExperienceWhy,
                selfCare = peer.SelfCare,

                requiredCourses = peer.RequiredCourses,

                supvrOrgName = peer.SupvrOrgName,
                supvrFirstName = peer.SupvrFirstName,
                supvrLastName = peer.SupvrLastName,
                supvrContAddr1 = peer.SupvrContAddr1,
                supvrContAddr2 = peer.SupvrContAddr2,
                supvrContCity = peer.SupvrContCity,
                supvrContState = peer.SupvrContState,
                supvrContZip = peer.SupvrContZip,
                supvrContPhone = peer.SupvrContPhone,
                supvrContEmail = peer.SupvrContEmail,
                complPracticum = peer.ComplPracticum,
                complPracticumMin = peer.ComplPracticumMin,
                practicumBdate = peer.PracticumBdate,
                practicumEdate = peer.PracticumEdate,

                examStatus = peer.ExamStatus,
                dateCompletion = peer.DateCompletion,
                dateCert = peer.DateCert,
                notes = peer.Notes,
                reasonDisapprv = peer.ReasonDisapprv,

                lastLoginDate = aspUser != null ? aspUser.LastLoginDate : null,
                lastCourseAttendedDate,

                certificationTracks = certificationTrack,
                uploads,
                exams
            });
        }
        [HttpPut("admin/manage-peer-detail/{userId:guid}")]
        public async Task<IActionResult> UpdateManagePeerDetail(Guid userId, [FromBody] JsonElement body)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var peer = await _context.PeerUsers.FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);
            if (peer == null)
                return NotFound(new { message = "Peer application not found." });

            static bool HasProp(JsonElement e, string name) =>
                e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out _);

            static string? GetString(JsonElement e, string name)
            {
                if (!e.TryGetProperty(name, out var p)) return null;
                if (p.ValueKind == JsonValueKind.Null) return null;
                var s = p.GetString();
                return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
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

            static int? GetInt(JsonElement e, string name)
            {
                if (!e.TryGetProperty(name, out var p)) return null;
                if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var v)) return v;
                if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var s)) return s;
                return null;
            }

            static DateTime? GetDate(JsonElement e, string name)
            {
                if (!e.TryGetProperty(name, out var p)) return null;
                if (p.ValueKind == JsonValueKind.String && DateTime.TryParse(p.GetString(), out var d)) return d;
                return null;
            }

            if (HasProp(body, "ApplicantNumber")) peer.ApplicantNumber = GetInt(body, "ApplicantNumber");
            if (HasProp(body, "Approve"))
                peer.Approve = GetBool(body, "Approve");

            if (HasProp(body, "Disapprove"))
                peer.Disapprove = GetBool(body, "Disapprove");

            if (HasProp(body, "Closed"))
                peer.Closed = GetBool(body, "Closed");

            if (HasProp(body, "Lapsed"))
                peer.Lapsed = GetBool(body, "Lapsed");

            if (HasProp(body, "Active"))
                peer.Active = GetBool(body, "Active");

            if (HasProp(body, "ApplicationPercentage"))
                peer.ApplicationPercentage = GetInt(body, "ApplicationPercentage");
            if (HasProp(body, "ReasonDisapprv")) peer.ReasonDisapprv = GetString(body, "ReasonDisapprv");
            if (HasProp(body, "Notes")) peer.Notes = GetString(body, "Notes");

            if (HasProp(body, "CertHivdate")) peer.CertHivdate = GetDate(body, "CertHivdate");
            if (HasProp(body, "CertHcvdate")) peer.CertHcvdate = GetDate(body, "CertHcvdate");
            if (HasProp(body, "CertHrdate")) peer.CertHrdate = GetDate(body, "CertHrdate");
            if (HasProp(body, "CertPrepDate")) peer.CertPrepDate = GetDate(body, "CertPrepDate");
            if (HasProp(body, "CertCriminalJusticeDate")) peer.CertCriminalJusticeDate = GetDate(body, "CertCriminalJusticeDate");
            if (HasProp(body, "ExperienceCommitment")) peer.ExperienceCommitment = GetString(body, "ExperienceCommitment");
            if (HasProp(body, "ExperienceChallenges")) peer.ExperienceChallenges = GetString(body, "ExperienceChallenges");
            if (HasProp(body, "ExperienceWhy")) peer.ExperienceWhy = GetString(body, "ExperienceWhy");
            if (HasProp(body, "SelfCare")) peer.SelfCare = GetBool(body, "SelfCare");

            if (HasProp(body, "RequiredCourses"))
            {
                var reqCourses = GetBool(body, "RequiredCourses");
                if (reqCourses.HasValue)
                    peer.RequiredCourses = reqCourses.Value;
            }

            if (HasProp(body, "SupvrOrgName")) peer.SupvrOrgName = GetString(body, "SupvrOrgName");
            if (HasProp(body, "SupvrFirstName")) peer.SupvrFirstName = GetString(body, "SupvrFirstName");
            if (HasProp(body, "SupvrLastName")) peer.SupvrLastName = GetString(body, "SupvrLastName");
            if (HasProp(body, "SupvrContAddr1")) peer.SupvrContAddr1 = GetString(body, "SupvrContAddr1");
            if (HasProp(body, "SupvrContAddr2")) peer.SupvrContAddr2 = GetString(body, "SupvrContAddr2");
            if (HasProp(body, "SupvrContPhone")) peer.SupvrContPhone = GetString(body, "SupvrContPhone");
            if (HasProp(body, "SupvrContEmail")) peer.SupvrContEmail = GetString(body, "SupvrContEmail");

            if (HasProp(body, "ComplPracticum")) peer.ComplPracticum = GetBool(body, "ComplPracticum");
            if (HasProp(body, "ComplPracticumMin")) peer.ComplPracticumMin = GetBool(body, "ComplPracticumMin");
            if (HasProp(body, "PracticumBDate")) peer.PracticumBdate = GetDate(body, "PracticumBDate");
            if (HasProp(body, "PracticumEDate")) peer.PracticumEdate = GetDate(body, "PracticumEDate");
            peer.DateModify = DateTime.UtcNow;

            var statusChangeDate = DateTime.UtcNow;

            // Closed
            if (peer.Closed == true)
            {
                peer.Approve = null;
                peer.Disapprove = null;
                peer.Lapsed = null;
                peer.Active = null;

                peer.ApprovedDt = null;
                peer.DisapprovedDt = null;
                peer.ReasonDisapprv = null;
            }

            // Lapsed
            else if (peer.Lapsed == true)
            {
                peer.Approve = null;
                peer.Disapprove = null;
                peer.Closed = null;
                peer.Active = null;

                peer.ApprovedDt = null;
                peer.DisapprovedDt = null;
                peer.ReasonDisapprv = null;
            }

            // Archived
            else if (peer.Active == false)
            {
                peer.Approve = null;
                peer.Disapprove = null;
                peer.Closed = null;
                peer.Lapsed = null;

                peer.ApprovedDt = null;
                peer.DisapprovedDt = null;
                peer.ReasonDisapprv = null;
            }

            // Approved
            else if (peer.Approve == true)
            {
                peer.Active = true;
                peer.Disapprove = null;
                peer.Closed = null;
                peer.Lapsed = null;

                peer.ApprovedDt = statusChangeDate;
                peer.DisapprovedDt = null;
                peer.ReasonDisapprv = null;
            }

            // Disapproved
            else if (peer.Disapprove == true)
            {
                peer.Active = true;
                peer.Approve = null;
                peer.Closed = null;
                peer.Lapsed = null;

                peer.DisapprovedDt = statusChangeDate;
                peer.ApprovedDt = null;
            }

            // Submitted
            else if (
                peer.Active == true &&
                peer.ApplicationPercentage == 100
            )
            {
                peer.Approve = null;
                peer.Disapprove = null;
                peer.Closed = null;
                peer.Lapsed = null;

                peer.ApprovedDt = null;
                peer.DisapprovedDt = null;
                peer.ReasonDisapprv = null;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Peer details updated successfully." });
        }
        [HttpGet("uploads/preview/{peerDocSysId:int}")]
        public async Task<IActionResult> PreviewUpload(int peerDocSysId)
        {
            var doc = await _context.PeerDocs.AsNoTracking()
                .FirstOrDefaultAsync(d => d.PeerDocSysId == peerDocSysId);

            if (doc == null || doc.Active != true)
                return NotFound(new { message = "Document not found." });

            if (!string.IsNullOrWhiteSpace(doc.DocPath) && doc.DocPath.TrimStart().StartsWith("{"))
                return BadRequest(new { message = "This record is not a file upload." });

            var blobName = doc.DocPath;
            if (string.IsNullOrWhiteSpace(blobName))
                return NotFound(new { message = "Missing blob path in DocPath." });

            var blob = _container.GetBlobClient(blobName);

            if (!await blob.ExistsAsync())
                return NotFound(new { message = "File missing in blob storage." });

            var fileName = !string.IsNullOrWhiteSpace(doc.DisplayFileName)
    ? doc.DisplayFileName
    : GetFileNameFromDocPath(blobName);

            var contentType = GuessContentType(fileName);

            var dl = await blob.DownloadStreamingAsync();

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";

            return File(dl.Value.Content, contentType, enableRangeProcessing: true);
        }

        [HttpPost("submit/{userId:guid}")]
        public async Task<IActionResult> SubmitApplication(Guid userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var peer = await _context.PeerUsers
                .FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);

            if (peer == null)
                return BadRequest(new { message = "Peer application not found. Please complete Step 1 first." });

            if (peer.CertHiv == true && peer.CertPrep == true)
            {
                return BadRequest(new { message = "HIV and PrEP certification tracks cannot be selected together." });
            }

            // required step 5 docs
            var requiredDocIds = new[] { 2, 3, 7, 8 };

            var uploadedRequiredDocs = await _context.PeerDocs
                .AsNoTracking()
                .Where(d => d.PeerSysId == peer.PeerSysId && d.Active == true && requiredDocIds.Contains(d.PeerDocId))
                .Select(d => d.PeerDocId)
                .Distinct()
                .ToListAsync();

            var missingDocs = requiredDocIds.Except(uploadedRequiredDocs).ToList();
            if (missingDocs.Any())
                return BadRequest(new { message = "Please complete all required uploads before submitting." });

            // validate step fields quickly
            if (string.IsNullOrWhiteSpace(peer.ExperienceCommitment) || peer.ExperienceCommitment.Trim().Length < 500 ||
                string.IsNullOrWhiteSpace(peer.ExperienceChallenges) || peer.ExperienceChallenges.Trim().Length < 500 ||
                string.IsNullOrWhiteSpace(peer.ExperienceWhy) || peer.ExperienceWhy.Trim().Length < 500)
            {
                return BadRequest(new { message = "Step 2 is incomplete." });
            }

            if (peer.RequiredCourses != true)
                return BadRequest(new { message = "Step 3 is incomplete." });

            if (string.IsNullOrWhiteSpace(peer.SupvrOrgName) ||
                string.IsNullOrWhiteSpace(peer.SupvrFirstName) ||
                string.IsNullOrWhiteSpace(peer.SupvrLastName) ||
                string.IsNullOrWhiteSpace(peer.SupvrContAddr1) ||
                string.IsNullOrWhiteSpace(peer.SupvrContPhone) ||
                string.IsNullOrWhiteSpace(peer.SupvrContEmail))
            {
                return BadRequest(new { message = "Step 4 is incomplete." });
            }

            if (peer.ComplPracticum == true)
            {
                if (peer.ComplPracticumMin != true || peer.PracticumBdate == null || peer.PracticumEdate == null)
                    return BadRequest(new { message = "Practicum details are incomplete." });
            }

            // validate only mandatory exams based on selected tracks
            var selectedTrackCodes = new List<string>();

            if (peer.CertHiv == true) selectedTrackCodes.Add("HIV");
            if (peer.CertHcv == true) selectedTrackCodes.Add("HCV");
            if (peer.CertHr == true) selectedTrackCodes.Add("HR");
            if (peer.CertPrep == true) selectedTrackCodes.Add("PREP");
            // CJ currently has no mapped exam, so do not include it unless you add one

            var requiredScormIds = PeerExamCourseMap
                .Where(x => selectedTrackCodes.Contains(x.Value.TrackCode))
                .Select(x => x.Value.CourseSysId)
                .Distinct()
                .ToList();


            Console.WriteLine("===== SUBMIT EXAM VALIDATION =====");
            Console.WriteLine($"UserSysId: {user.UserSysId}");
            Console.WriteLine($"PeerSysId: {peer.PeerSysId}");
            Console.WriteLine($"CertHiv: {peer.CertHiv}");
            Console.WriteLine($"CertHcv: {peer.CertHcv}");
            Console.WriteLine($"CertHr: {peer.CertHr}");
            Console.WriteLine($"CertPrep: {peer.CertPrep}");
            Console.WriteLine($"CertCJ: {peer.CertCriminalJustice}");

            Console.WriteLine(
                $"Selected Tracks: {string.Join(", ", selectedTrackCodes)}"
            );

            Console.WriteLine(
                $"Required SCORM IDs: {string.Join(", ", requiredScormIds)}"
            );


            if (requiredScormIds.Any())
            {
                var sessions = await _context.ScormAiccSessions
                    .AsNoTracking()
                    .Where(x => x.Userid == user.UserSysId && requiredScormIds.Contains(x.Scormid))
                    .GroupBy(x => x.Scormid)
                    .Select(g => g
                        .OrderByDescending(x => x.Attempt)
                        .ThenByDescending(x => x.Timemodified)
                        .FirstOrDefault())
                    .ToListAsync();

                var completedScormIds = sessions
                    .Where(s => s != null &&
                        (
                            string.Equals(s.Scormstatus, "completed", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s.Lessonstatus, "completed", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s.Lessonstatus, "passed", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s.Lessonstatus, "failed", StringComparison.OrdinalIgnoreCase)
                        ))
                    .Select(s => s.Scormid)
                    .Distinct()
                    .ToList();

                Console.WriteLine(
    $"Completed SCORM IDs: {string.Join(", ", completedScormIds)}"
);

                Console.WriteLine(
                    $"Required Count: {requiredScormIds.Count}"
                );

                Console.WriteLine(
                    $"Completed Count: {completedScormIds.Count}"
                );

                if (completedScormIds.Count != requiredScormIds.Count)
                    return BadRequest(new { message = "Please complete all mandatory certification exams before submitting." });
            }

            // FINAL SUCCESS: activate only here
            var submittedDate = DateTime.UtcNow;

            peer.Active = true;
            peer.ApplicationPercentage = 100;
            peer.DateModify = submittedDate;

            await _context.SaveChangesAsync();

            var certificationTracks = new List<string>();

            if (peer.CertHiv == true)
                certificationTracks.Add("HIV");

            if (peer.CertHcv == true)
                certificationTracks.Add("HCV");

            if (peer.CertHr == true)
                certificationTracks.Add("Harm Reduction");

            if (peer.CertPrep == true)
                certificationTracks.Add("PrEP");

            if (peer.CertCriminalJustice == true)
                certificationTracks.Add("Criminal Justice");

            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                fullName = "Applicant";

            var emailSent = false;
            string? emailWarning = null;

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var emailSubject =
                    "Peer Certification Application Submitted Successfully";

                var emailBody = BuildPeerSubmissionEmailTemplate(
                    fullName,
                    certificationTracks,
                    submittedDate
                );

                try
                {
                    await _emailService.SendEmailAsync(
                        user.Email,
                        emailSubject,
                        emailBody
                    );

                    emailSent = true;
                }
                catch (Exception ex)
                {
                    // Application remains successfully submitted even if email fails.
                    emailWarning =
                        "The application was submitted, but the confirmation email could not be sent.";

                    Console.WriteLine(
                        $"Peer submission email failed for UserSysId " +
                        $"{user.UserSysId}: {ex.Message}"
                    );
                }
            }
            else
            {
                emailWarning =
                    "The application was submitted, but no email address was available.";
            }

            return Ok(new
            {
                message = "Peer certification application submitted successfully.",
                active = peer.Active,
                applicationPercentage = peer.ApplicationPercentage,
                emailSent,
                emailWarning
            });
        }

        [HttpGet("uploads/{userId:guid}")]
        public async Task<IActionResult> GetUploads(Guid userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });

            var peer = await _context.PeerUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);

            if (peer == null)
                return Ok(new { peerSysId = (int?)null, docs = Array.Empty<object>() });

            // ✅ Only return blob-backed records
            // - excludes ethics JSON (starts with "{")
            // - excludes old disk paths (/Users/...)
            // - includes only new blob paths (peeruploads/...)
            var rows = await (
                from d in _context.PeerDocs.AsNoTracking()
                join t in _context.LkPeerDocTypes.AsNoTracking()
                    on d.PeerDocId equals t.PeerDocId
                where d.PeerSysId == peer.PeerSysId
                      && d.Active == true
                      && t.Active == true
                      && d.DocPath != null
                      && !d.DocPath.StartsWith("{")
                      && d.DocPath.StartsWith("peeruploads/")
                orderby d.DateUpload descending
                select new
                {
                    d.PeerDocSysId,
                    d.PeerDocId,
                    docTypeName = t.Name,
                    d.DocPath,
                    d.DateUpload,
                    d.Reviewed
                }
            ).ToListAsync();

            var docs = rows.Select(x => new
            {
                x.PeerDocSysId,
                x.PeerDocId,
                x.docTypeName,
                fileName = GetFileNameFromDocPath(x.DocPath),
                x.DateUpload,
                x.Reviewed
            }).ToList();

            return Ok(new { peerSysId = peer.PeerSysId, docs });
        }
        private static string GetFileNameFromDocPath(string? docPath)
        {
            if (string.IsNullOrWhiteSpace(docPath)) return "";

            // ethics JSON record (not a file)
            if (docPath.TrimStart().StartsWith("{")) return "";

            var idx = docPath.LastIndexOf('/');
            if (idx >= 0 && idx < docPath.Length - 1)
                return docPath.Substring(idx + 1);

            return Path.GetFileName(docPath);
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

            // validate doc type exists and active
            var dt = await _context.LkPeerDocTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PeerDocId == docType && x.Active == true);

            if (dt == null)
                return BadRequest(new { message = "Invalid document type." });
            await EnsureContainerAsync();

            // Soft-deactivate previous docs for same type (optional rule)
            var existingDocs = await _context.PeerDocs
                .Where(d => d.PeerSysId == peer.PeerSysId && d.PeerDocId == docType && d.Active == true)
                .ToListAsync();

            foreach (var d in existingDocs)
            {
                d.Active = false;
                d.DateModify = DateTime.UtcNow;
            }

            // Build blob name
            var originalSafe = SafeBlobSegment(Path.GetFileName(file.FileName));
            var ext2 = Path.GetExtension(originalSafe).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext2)) ext2 = ext;

            var storedName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{ext2}";
            var blobName = $"peeruploads/{peer.PeerSysId}/{docType}/{storedName}";

            // Upload to blob
            var blob = _container.GetBlobClient(blobName);

            await using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? GuessContentType(originalSafe)
                        : file.ContentType
                });
            }

            // Save DB record with DocPath = blobName
            var doc = new PeerDoc
            {
                PeerSysId = peer.PeerSysId,
                PeerDocId = docType,
                DocType = docType,

                DocPath = blobName,
                DisplayFileName = originalSafe,

                DateUpload = DateTime.UtcNow,
                Active = true,
                UploadBy = user.Email ?? user.UserSysId.ToString(),

                Reviewed = false,
                ReviewStatus = (int)EduCreditReviewStatus.Pending,
                AdminComments = null,

                NoOfCredits = null
            };

            _context.PeerDocs.Add(doc);
            await _context.SaveChangesAsync();

            return await GetUploads(userId);
        }

        [HttpGet("uploads/download/{peerDocSysId:int}")]
        public async Task<IActionResult> Download(int peerDocSysId)
        {
            var doc = await _context.PeerDocs.AsNoTracking()
                .FirstOrDefaultAsync(d => d.PeerDocSysId == peerDocSysId);

            if (doc == null || doc.Active != true)
                return NotFound(new { message = "Document not found." });

            if (!string.IsNullOrWhiteSpace(doc.DocPath) && doc.DocPath.TrimStart().StartsWith("{"))
                return BadRequest(new { message = "This record is not a file upload." });

            var blobName = doc.DocPath;
            if (string.IsNullOrWhiteSpace(blobName))
                return NotFound(new { message = "Missing blob path in DocPath." });

            var blob = _container.GetBlobClient(blobName);

            if (!await blob.ExistsAsync())
                return NotFound(new { message = "File missing in blob storage." });

            var fileName = blobName.Contains("/")
                ? blobName.Split('/').Last()
                : Path.GetFileName(blobName);

            var contentType = GuessContentType(fileName);

            var dl = await blob.DownloadStreamingAsync();
            return File(dl.Value.Content, contentType, fileName);
        }
        [HttpDelete("uploads/{userId:guid}/{peerDocSysId:int}")]
        public async Task<IActionResult> DeleteUpload(Guid userId, int peerDocSysId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound(new { message = "User not found" });

            var peer = await _context.PeerUsers.FirstOrDefaultAsync(p => p.UserSysId == user.UserSysId);
            if (peer == null) return NotFound(new { message = "Peer record not found" });

            var doc = await _context.PeerDocs
                .FirstOrDefaultAsync(d => d.PeerDocSysId == peerDocSysId && d.PeerSysId == peer.PeerSysId);

            if (doc == null) return NotFound(new { message = "Document not found" });

            // Try delete blob (best effort)
            try
            {
                if (!string.IsNullOrWhiteSpace(doc.DocPath) && !doc.DocPath.TrimStart().StartsWith("{"))
                {
                    var blob = _container.GetBlobClient(doc.DocPath);
                    await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                }
            }
            catch
            {
                // ignore storage errors; still soft delete DB
            }

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


        private static string BuildPeerSubmissionEmailTemplate(
        string fullName,
        IEnumerable<string> certificationTracks,
        DateTime submittedDate)
        {
            var safeFullName = WebUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(fullName)
                    ? "Applicant"
                    : fullName
            );

            var trackList = certificationTracks?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(WebUtility.HtmlEncode)
                .ToList() ?? new List<string>();

            var trackText = trackList.Any()
                ? string.Join(", ", trackList)
                : "Not specified";

            var submittedDateText = submittedDate.ToString("MM/dd/yyyy hh:mm tt");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
</head>

<body style='
    margin:0;
    padding:0;
    background:#f4f4f7;
    font-family:Segoe UI, Arial, sans-serif;'>

<table
    width='100%'
    cellpadding='0'
    cellspacing='0'
    role='presentation'
    style='background:#f4f4f7; padding:30px 12px;'>

<tr>
<td align='center'>

<table
    width='640'
    cellpadding='0'
    cellspacing='0'
    role='presentation'
    style='
        width:100%;
        max-width:640px;
        background:#ffffff;
        border-radius:14px;
        overflow:hidden;
        box-shadow:0 8px 24px rgba(0,0,0,0.08);'>

<tr>
<td style='
    background:#43285D;
    padding:24px 30px;
    color:#ffffff;'>

<h2 style='
    margin:0;
    font-size:24px;
    line-height:1.3;'>

HIV Training Portal

</h2>

<p style='
    margin:6px 0 0;
    font-size:14px;
    color:#f3e8ff;'>

Peer Certification Application

</p>

</td>
</tr>

<tr>
<td style='padding:30px; color:#333333;'>

<h3 style='
    margin:0 0 20px;
    color:#43285D;
    font-size:20px;'>

Hello {safeFullName},

</h3>

<div style='
    padding:16px 18px;
    background:#ecfdf3;
    border-left:5px solid #16a34a;
    border-radius:10px;
    color:#166534;
    font-size:16px;
    font-weight:700;
    margin-bottom:22px;'>

Your Peer Certification application was submitted successfully.

</div>

<p style='
    margin:0 0 16px;
    font-size:15px;
    line-height:1.7;'>

Your application and supporting documents have been received and are now pending administrative review.

</p>

<p style='
    margin:0 0 24px;
    font-size:15px;
    line-height:1.7;'>

No further action is required from you at this time. Please allow the Peer Certification team time to review your submission. You will receive another notification after a decision has been made.

</p>

<h3 style='
    color:#43285D;
    margin:28px 0 12px;
    font-size:18px;'>

Submission Details

</h3>

<table
    width='100%'
    cellpadding='0'
    cellspacing='0'
    role='presentation'
    style='
        border-collapse:collapse;
        border:1px solid #e5e7eb;
        border-radius:10px;'>

<tr>
<td style='
    width:190px;
    padding:12px;
    background:#f8f7fa;
    border-bottom:1px solid #e5e7eb;
    font-weight:600;'>

Application Status

</td>

<td style='
    padding:12px;
    border-bottom:1px solid #e5e7eb;'>

Submitted – Pending Review

</td>
</tr>

<tr>
<td style='
    width:190px;
    padding:12px;
    background:#f8f7fa;
    border-bottom:1px solid #e5e7eb;
    font-weight:600;'>

Certification Track(s)

</td>

<td style='
    padding:12px;
    border-bottom:1px solid #e5e7eb;'>

{trackText}

</td>
</tr>

<tr>
<td style='
    width:190px;
    padding:12px;
    background:#f8f7fa;
    font-weight:600;'>

Submitted On

</td>

<td style='padding:12px;'>

{WebUtility.HtmlEncode(submittedDateText)}

</td>
</tr>

</table>

<p style='
    margin-top:28px;
    font-size:15px;
    line-height:1.6;'>

Thank you,<br/>

<strong>HIV Training Support Team</strong><br/>

New York State Department of Health

</p>

</td>
</tr>

<tr>
<td style='
    background:#f4eff9;
    padding:16px 30px;
    font-size:12px;
    color:#6b7280;
    text-align:center;'>

This is an automated message. Please do not reply to this email.

</td>
</tr>

</table>

</td>
</tr>
</table>

</body>
</html>";
        }
    }
}
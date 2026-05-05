using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System.IO.Compression;
namespace HIVTraining_Vue.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingTitleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobContainerClient _titleImageContainer;
        private bool _titleImageContainerReady = false;

        private readonly BlobContainerClient _scormContainer;
        private bool _scormContainerReady = false;


        public TrainingTitleController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;

            var cs = config["Storage:ConnectionString"];
            var containerName = config["Storage:TitleImagesContainerName"] ?? "title-images";

            var serviceClient = new BlobServiceClient(cs);
            _titleImageContainer = serviceClient.GetBlobContainerClient(containerName);

            var scormContainerName = config["Storage:ScormContainerName"] ?? "scorm-packages";
            _scormContainer = serviceClient.GetBlobContainerClient(scormContainerName);
        }

        private async Task EnsureTitleImageContainerAsync()
        {
            if (_titleImageContainerReady) return;

            await _titleImageContainer.CreateIfNotExistsAsync(PublicAccessType.None);
            _titleImageContainerReady = true;
        }

        private async Task EnsureScormContainerAsync()
        {
            if (_scormContainerReady) return;

            await _scormContainer.CreateIfNotExistsAsync(PublicAccessType.None);
            _scormContainerReady = true;
        }

        private static readonly string[] AllowedImageExt = new[] { ".png", ".jpg", ".jpeg", ".webp" };
        private const long MaxTitleImageBytes = 2 * 1024 * 1024; // 2MB

        private static string GuessContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            return provider.TryGetContentType(fileName, out var ct) ? ct : "application/octet-stream";
        }

        [HttpPost("{subjectId:int}/image")]
        [RequestSizeLimit(MaxTitleImageBytes)]
        public async Task<IActionResult> UploadTitleImage(int subjectId, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            if (file.Length > MaxTitleImageBytes)
                return BadRequest(new { message = "Image too large (max 2MB)." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExt.Contains(ext))
                return BadRequest(new { message = "Invalid image type. Allowed: png, jpg, jpeg, webp." });

            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectSysId == subjectId);
            if (subject == null)
                return NotFound(new { message = "Title not found." });

            await EnsureTitleImageContainerAsync();

            // delete old image (optional but recommended)
            if (!string.IsNullOrWhiteSpace(subject.TitleImagePath))
            {
                try
                {
                    await _titleImageContainer.GetBlobClient(subject.TitleImagePath).DeleteIfExistsAsync();
                }
                catch { /* ignore */ }
            }

            var blobName = $"titles/{subjectId}/{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{ext}";
            var blob = _titleImageContainer.GetBlobClient(blobName);

            await using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? GuessContentType(file.FileName)
                        : file.ContentType
                });
            }

            subject.TitleImagePath = blobName;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Image uploaded!", subjectId, blobName });
        }

        [HttpPost("{subjectId:int}/scorm-package")]
        [RequestSizeLimit(200 * 1024 * 1024)]
        public async Task<IActionResult> UploadScormPackage(int subjectId, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No SCORM package uploaded." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".zip")
                return BadRequest(new { message = "Only ZIP files are allowed for SCORM packages." });

            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectSysId == subjectId);
            if (subject == null)
                return NotFound(new { message = "Training title not found." });

            await EnsureScormContainerAsync();

            var packageFolder = $"scorm/{subjectId}/{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
            var zipBlobName = $"{packageFolder}/package.zip";

            var zipBlob = _scormContainer.GetBlobClient(zipBlobName);

            await using (var stream = file.OpenReadStream())
            {
                await zipBlob.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = "application/zip"
                });
            }

            using var memory = new MemoryStream();
            await using (var input = file.OpenReadStream())
            {
                await input.CopyToAsync(memory);
            }

            memory.Position = 0;

            string? launchFile = null;

            using (var archive = new ZipArchive(memory, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.Name))
                        continue;

                    var safeEntryName = entry.FullName.Replace("\\", "/");

                    if (safeEntryName.Contains(".."))
                        continue;

                    var entryBlobName = $"{packageFolder}/content/{safeEntryName}";
                    var entryBlob = _scormContainer.GetBlobClient(entryBlobName);

                    await using var entryStream = entry.Open();

                    await entryBlob.UploadAsync(entryStream, new BlobHttpHeaders
                    {
                        ContentType = GuessContentType(entry.Name)
                    });

                    var lower = safeEntryName.ToLowerInvariant();

                    if (launchFile == null &&
                        (lower.EndsWith("index.html") ||
                         lower.EndsWith("index.htm") ||
                         lower.EndsWith("story.html") ||
                         lower.EndsWith("story_html5.html")))
                    {
                        launchFile = safeEntryName;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(launchFile))
                return BadRequest(new { message = "Could not find a launch file like index.html, story.html, or story_html5.html inside the SCORM ZIP." });

            subject.IsOnlineTraining = true;
            subject.VideoUrl = $"/api/TrainingTitle/scorm-launch/{subjectId}";
            await _context.SaveChangesAsync();

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.SubjectSysId == subjectId);
            if (course != null)
            {
                course.VirtualUrl = subject.VideoUrl;
                course.DateModified = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "SCORM package uploaded successfully.",
                launchUrl = subject.VideoUrl,
                launchFile
            });
        }

        [HttpGet("scorm-launch/{subjectId:int}")]
        public async Task<IActionResult> LaunchScorm(int subjectId)
        {
            var subject = await _context.Subjects.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SubjectSysId == subjectId);

            if (subject == null)
                return NotFound("Training title not found.");

            var prefix = $"scorm/{subjectId}/";

            await EnsureScormContainerAsync();

            await foreach (var blobItem in _scormContainer.GetBlobsAsync(
                BlobTraits.None,
                BlobStates.None,
                prefix,
                default))
            {
                var name = blobItem.Name.Replace("\\", "/");
                var lower = name.ToLowerInvariant();

                if (!lower.Contains("/content/"))
                    continue;

                if (lower.EndsWith("index.html") ||
                    lower.EndsWith("index.htm") ||
                    lower.EndsWith("story.html") ||
                    lower.EndsWith("story_html5.html"))
                {
                    var marker = "/content/";
                    var relativePath = name.Substring(name.IndexOf(marker) + marker.Length);

                    return Redirect($"/api/TrainingTitle/scorm-content/{subjectId}/{relativePath}");
                }
            }

            return NotFound("SCORM launch file not found.");
        }

        [HttpGet("scorm-content/{subjectId:int}/{*path}")]
        public async Task<IActionResult> GetScormContent(int subjectId, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Contains(".."))
                return BadRequest("Invalid SCORM file path.");

            var prefix = $"scorm/{subjectId}/";

            string? matchedBlobName = null;
            await EnsureScormContainerAsync();
            await foreach (var blobItem in _scormContainer.GetBlobsAsync(
    BlobTraits.None,
    BlobStates.None,
    prefix,
    default))
            {
                if (blobItem.Name.EndsWith($"/content/{path}", StringComparison.OrdinalIgnoreCase))
                {
                    matchedBlobName = blobItem.Name;
                    break;
                }
            }

            if (matchedBlobName == null)
                return NotFound("SCORM file not found.");

            var blob = _scormContainer.GetBlobClient(matchedBlobName);

            if (!await blob.ExistsAsync())
                return NotFound("SCORM file missing.");

            var fileName = Path.GetFileName(path);
            var contentType = GuessContentType(fileName);

            var dl = await blob.DownloadStreamingAsync();

            return File(dl.Value.Content, contentType, enableRangeProcessing: true);
        }



        [HttpGet("{subjectId:int}/image")]
        public async Task<IActionResult> GetTitleImage(int subjectId)
        {
            var subject = await _context.Subjects.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SubjectSysId == subjectId);

            if (subject == null)
                return NotFound(new { message = "Title not found." });

            if (string.IsNullOrWhiteSpace(subject.TitleImagePath))
                return NotFound(new { message = "No image uploaded for this title." });

            var blob = _titleImageContainer.GetBlobClient(subject.TitleImagePath);

            if (!await blob.ExistsAsync())
                return NotFound(new { message = "Image missing in blob storage." });

            var fileName = subject.TitleImagePath.Split('/').Last();
            var contentType = GuessContentType(fileName);

            var dl = await blob.DownloadStreamingAsync();
            return File(dl.Value.Content, contentType, fileName);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTitle([FromBody] JsonElement body)
        {
            try
            {
                var courseTitle = body.GetProperty("courseTitle").GetString();
                if (string.IsNullOrWhiteSpace(courseTitle))
                    return BadRequest(new { message = "Course title is required." });

                var subject = new Subject
                {
                    CourseTitle = courseTitle,
                    Description = body.TryGetProperty("description", out var d) ? d.GetString() : null,
                    Cnecredits = body.TryGetProperty("cnecredits", out var cne) && cne.GetBoolean(),
                    Oasascredits = body.TryGetProperty("oasascredits", out var oa) && oa.GetBoolean(),
                    CertDescription = body.TryGetProperty("certDescription", out var cd) ? cd.GetString() : null,
                    MiscCertDesc = body.TryGetProperty("miscCertDesc", out var md) ? md.GetString() : null,
                    VideoUrl = body.TryGetProperty("videoUrl", out var vu) ? vu.GetString() : null,
                    IsOnlineTraining = body.TryGetProperty("isOnlineTraining", out var ot) && ot.GetBoolean(),
                    Active = true,
                    MarkAsNewUntil = body.TryGetProperty("markAsNewUntil", out var mu) && mu.ValueKind != JsonValueKind.Null
                        ? DateTime.Parse(mu.GetString()!)
                        : null
                };

                // read topicCodes: [1,3,7]
                var topicCodes = new List<int>();
                if (body.TryGetProperty("topicCodes", out var tc) && tc.ValueKind == JsonValueKind.Array)
                {
                    foreach (var x in tc.EnumerateArray())
                        topicCodes.Add(x.GetInt32());
                }

                topicCodes = topicCodes.Distinct().ToList();

                // validate topics
                if (topicCodes.Count > 0)
                {
                    var validCount = await _context.LkTopics.CountAsync(t => topicCodes.Contains(t.Code));
                    if (validCount != topicCodes.Count)
                        return BadRequest(new { message = "One or more selected topics are invalid." });
                }

                if (topicCodes.Count == 0)
                    return BadRequest(new { message = "Please select at least one topic." });

                var strategy = _context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _context.Database.BeginTransactionAsync();

                    _context.Subjects.Add(subject);
                    await _context.SaveChangesAsync();

                    // insert join rows
                    foreach (var code in topicCodes)
                    {
                        _context.SubjectTopics.Add(new SubjectTopic
                        {
                            SubjectSysId = subject.SubjectSysId,
                            TopicCode = code
                        });
                    }
                    await _context.SaveChangesAsync();

                    // auto-create course for online
                    if (subject.IsOnlineTraining)
                    {
                        var siteId = await _context.Sites.Where(s => s.Active)
                            .Select(s => s.SiteSysId).FirstOrDefaultAsync();

                        if (siteId == 0) throw new Exception("No active Site found.");

                        int? onlineFormatCode = await _context.LkFormats
                            .Where(f => f.Value != null && f.Value.ToLower().Contains("online"))
                            .Select(f => (int?)f.Code)
                            .FirstOrDefaultAsync();

                        _context.Courses.Add(new Course
                        {
                            SiteSysId = siteId,
                            SubjectSysId = subject.SubjectSysId,
                            Hidden = false,
                            VirtualUrl = subject.VideoUrl,
                            MaxSeats = 99999,
                            Format = onlineFormatCode,
                            DateEntered = DateTime.UtcNow,
                            DateModified = DateTime.UtcNow,
                            MarkAsNewUntil = subject.MarkAsNewUntil
                        });

                        await _context.SaveChangesAsync();
                    }

                    await tx.CommitAsync();
                });

                return Ok(new { message = "Title created successfully!", subjectId = subject.SubjectSysId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create title.", error = ex.Message });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedTitles([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? title = null)
        {
            var baseQuery = _context.Subjects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                baseQuery = baseQuery.Where(s => s.CourseTitle!.Contains(title));

            var total = await baseQuery.CountAsync();

            var subjects = await baseQuery
                .OrderBy(s => s.CourseTitle)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new { s.SubjectSysId, s.CourseTitle })
                .ToListAsync();

            var ids = subjects.Select(s => s.SubjectSysId).ToList();

            var topicMap = await (
                from st in _context.SubjectTopics
                join t in _context.LkTopics on st.TopicCode equals t.Code
                where ids.Contains(st.SubjectSysId)
                select new { st.SubjectSysId, t.Code, t.Value }
            ).ToListAsync();

            var data = subjects.Select(s => new
            {
                s.SubjectSysId,
                s.CourseTitle,
                Topics = topicMap
                    .Where(x => x.SubjectSysId == s.SubjectSysId)
                    .Select(x => new { x.Code, x.Value })
                    .ToList()
            });

            return Ok(new { total, data });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTitleById(int id)
        {
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectSysId == id);
            if (subject == null) return NotFound();

            var topicCodes = await _context.SubjectTopics
                .Where(st => st.SubjectSysId == id)
                .Select(st => st.TopicCode)
                .ToListAsync();

            return Ok(new
            {
                subject.SubjectSysId,
                subject.CourseTitle,
                subject.Description,
                subject.Cnecredits,
                subject.Oasascredits,
                subject.CertDescription,
                subject.MiscCertDesc,
                subject.VideoUrl,
                subject.IsOnlineTraining,
                subject.MarkAsNewUntil,
                topicCodes,

                hasTitleImage = !string.IsNullOrWhiteSpace(subject.TitleImagePath),
                titleImagePath = subject.TitleImagePath
            });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateTitle(int id, [FromBody] JsonElement body)
        {
            try
            {
                var existing = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectSysId == id);
                if (existing == null)
                    return NotFound(new { message = "Title not found." });

                // capture old value BEFORE overwrite
                bool wasOnline = existing.IsOnlineTraining;

                string? courseTitle = body.TryGetProperty("courseTitle", out var ct) ? ct.GetString() : null;
                if (string.IsNullOrWhiteSpace(courseTitle))
                    return BadRequest(new { message = "Course title is required." });



                existing.CourseTitle = courseTitle;
                existing.Description = body.TryGetProperty("description", out var d) ? d.GetString() : null;

                existing.Cnecredits = body.TryGetProperty("cnecredits", out var cne) && cne.ValueKind == JsonValueKind.True;
                existing.Oasascredits = body.TryGetProperty("oasascredits", out var oa) && oa.ValueKind == JsonValueKind.True;

                existing.CertDescription = body.TryGetProperty("certDescription", out var cd) ? cd.GetString() : null;
                existing.MiscCertDesc = body.TryGetProperty("miscCertDesc", out var md) ? md.GetString() : null;

                existing.VideoUrl = body.TryGetProperty("videoUrl", out var vu) ? vu.GetString() : null;

                bool newIsOnline = body.TryGetProperty("isOnlineTraining", out var ot) && ot.ValueKind == JsonValueKind.True;
                existing.IsOnlineTraining = newIsOnline;

                existing.MarkAsNewUntil =
                    body.TryGetProperty("markAsNewUntil", out var mu) && mu.ValueKind != JsonValueKind.Null && !string.IsNullOrWhiteSpace(mu.GetString())
                        ? DateTime.Parse(mu.GetString()!)
                        : null;

                // ---- topicCodes (required) ----
                var topicCodes = new List<int>();
                if (body.TryGetProperty("topicCodes", out var tc) && tc.ValueKind == JsonValueKind.Array)
                {
                    foreach (var x in tc.EnumerateArray())
                        if (x.ValueKind == JsonValueKind.Number) topicCodes.Add(x.GetInt32());
                }
                topicCodes = topicCodes.Distinct().ToList();

                if (topicCodes.Count == 0)
                    return BadRequest(new { message = "Please select at least one topic." });

                var validCount = await _context.LkTopics.CountAsync(t => topicCodes.Contains(t.Code));
                if (validCount != topicCodes.Count)
                    return BadRequest(new { message = "One or more selected topics are invalid." });


                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _context.Database.BeginTransactionAsync();

                    // 1) Update Subject row
                    await _context.SaveChangesAsync();

                    // 2) Replace join rows in SubjectTopics
                    var existingLinks = await _context.SubjectTopics
                        .Where(st => st.SubjectSysId == id)
                        .ToListAsync();

                    _context.SubjectTopics.RemoveRange(existingLinks);
                    await _context.SaveChangesAsync();

                    foreach (var code in topicCodes)
                    {
                        _context.SubjectTopics.Add(new SubjectTopic
                        {
                            SubjectSysId = id,
                            TopicCode = code
                        });
                    }
                    await _context.SaveChangesAsync();

                    // ✅ 3) If changed from NOT online -> online, create/update Course
                    if (newIsOnline)
                    {
                        var siteId = await _context.Sites
                            .Where(s => s.Active)
                            .Select(s => s.SiteSysId)
                            .FirstOrDefaultAsync();

                        if (siteId == 0) throw new Exception("No active Site found.");

                        int? onlineFormatCode = await _context.LkFormats
                            .Where(f => f.Value != null && f.Value.ToLower().Contains("online"))
                            .Select(f => (int?)f.Code)
                            .FirstOrDefaultAsync();

                        // if course already exists for this subject, update it; else create
                        var existingCourse = await _context.Courses
                            .FirstOrDefaultAsync(c => c.SubjectSysId == id);

                        if (existingCourse == null)
                        {
                            _context.Courses.Add(new Course
                            {
                                SiteSysId = siteId,
                                SubjectSysId = id,
                                Hidden = false,
                                VirtualUrl = existing.VideoUrl,
                                Format = onlineFormatCode,
                                DateEntered = DateTime.UtcNow,
                                DateModified = DateTime.UtcNow,
                                MaxSeats = 99999,
                                MarkAsNewUntil = existing.MarkAsNewUntil
                            });
                        }
                        else
                        {
                            existingCourse.SiteSysId = siteId;
                            existingCourse.Hidden = false;
                            existingCourse.VirtualUrl = existing.VideoUrl;
                            existingCourse.Format = onlineFormatCode;
                            existingCourse.DateModified = DateTime.UtcNow;
                            existingCourse.MarkAsNewUntil = existing.MarkAsNewUntil;
                            existingCourse.MaxSeats = 99999;
                        }

                        await _context.SaveChangesAsync();
                    }


                    await tx.CommitAsync();
                });

                return Ok(new { message = "Title updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update title.", error = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteTitle(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound();

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Title deleted successfully!" });
        }
    }
}
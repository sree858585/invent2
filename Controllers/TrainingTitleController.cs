using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace HIVTraining_Vue.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingTitleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainingTitleController(ApplicationDbContext context)
        {
            _context = context;
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
                topicCodes
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
                    if (!wasOnline && newIsOnline)
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
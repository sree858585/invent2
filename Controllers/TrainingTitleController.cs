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
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
                return NotFound();

            return Ok(subject);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateTitle(int id, [FromBody] Subject updated)
        {
            if (updated == null)
                return BadRequest(new { message = "Request body is required." });

            var existing = await _context.Subjects.FindAsync(id);
            if (existing == null)
                return NotFound();

            // ✅ Category no longer used
            // existing.Category = updated.Category;

            // ✅ Validate TopicCode if provided
            if (updated.TopicCode.HasValue)
            {
                var topicExists = await _context.LkTopics
                    .AnyAsync(t => t.Code == updated.TopicCode.Value);

                if (!topicExists)
                    return BadRequest(new { message = "Invalid topic selected." });
            }

            existing.CourseTitle = updated.CourseTitle;
            existing.Description = updated.Description;

            existing.TopicCode = updated.TopicCode; // ✅ NEW

            existing.Active = updated.Active;
            existing.Ai = updated.Ai;
            existing.Cnecredits = updated.Cnecredits;
            existing.Oasascredits = updated.Oasascredits;
            existing.CreditHrs = updated.CreditHrs;
            existing.Is3rdParty = updated.Is3rdParty;
            existing.A3rdPartyCrseId = updated.A3rdPartyCrseId;
            existing.CertDescription = updated.CertDescription;
            existing.MiscCertDesc = updated.MiscCertDesc;
            existing.VideoUrl = updated.VideoUrl;
            existing.IsOnlineTraining = updated.IsOnlineTraining;
            existing.MarkAsNewUntil = updated.MarkAsNewUntil;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Title updated successfully!" });
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
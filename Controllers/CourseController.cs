// using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Globalization;

namespace HIVTraining.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;


        public CourseController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;

        }

        [HttpGet("FormatPaged/{format}")]
        public async Task<IActionResult> GetCoursesByFormatPaged(
    int format,                                    // legacy: 0 = All, >0 = single format
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 9,
    [FromQuery] string? search = null,
    [FromQuery] int? region = null,
    [FromQuery] int? category = null,
    [FromQuery] int? site = null,
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null,
    [FromQuery] string? formats = null            // NEW: multi-select "1,2,4"
)
        {
            var baseQuery = _context.Courses
                .Where(c => !c.Hidden);

            // ===== FORMAT FILTERS =====
            // Parse multi-select first
            List<int> selectedFormats = new();
            if (!string.IsNullOrWhiteSpace(formats))
            {
                selectedFormats = formats
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => int.TryParse(s, out var v) ? (int?)v : null)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .Distinct()
                    .ToList();
            }

            if (selectedFormats.Count > 0)
            {
                // multi-select overrides legacy param
                baseQuery = baseQuery.Where(c => c.Format.HasValue && selectedFormats.Contains(c.Format.Value));
            }
            else if (format != 0)
            {
                // legacy single-format
                baseQuery = baseQuery.Where(c => c.Format == format);
            }

            // ===== SEARCH =====
            if (!string.IsNullOrWhiteSpace(search))
            {
                baseQuery = baseQuery.Where(c =>
                    (c.Subject != null && EF.Functions.Like(c.Subject.CourseTitle, $"%{search}%")) ||
                    (c.Subject != null && EF.Functions.Like(c.Subject.Description, $"%{search}%")) ||
                    (c.City != null && EF.Functions.Like(c.City, $"%{search}%")) ||
                    (c.Information != null && EF.Functions.Like(c.Information, $"%{search}%"))
                );
            }

            // ===== ADVANCED FILTERS =====
            if (region.HasValue)
                baseQuery = baseQuery.Where(c => c.Region == region);

            if (category.HasValue)
                baseQuery = baseQuery.Where(c => c.ContractType == category);

            if (site.HasValue)
                baseQuery = baseQuery.Where(c => c.SiteSysId == site);

            if (fromDate.HasValue)
                baseQuery = baseQuery.Where(c => c.CourseDate >= fromDate.Value);

            if (toDate.HasValue)
                baseQuery = baseQuery.Where(c => c.CourseDate <= toDate.Value);

            // ===== INCLUDE & PAGE =====
            var query = baseQuery.Include(c => c.Subject);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(c => c.CourseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.CourseSysId,
                    c.CourseDate,
                    c.CourseTime,
                    c.Information,
                    c.City,
                    c.TrainingLocation,
                    c.MaxSeats,
                    c.Format,
                    c.Region,
                    c.ContractType,
                    c.Instructor1,
                    c.Instructor2,
                    IsMultiSession = c.IsMultiSession,
                    SiteName = _context.Sites
                        .Where(s => s.SiteSysId == c.SiteSysId)
                        .Select(s => s.SiteName)
                        .FirstOrDefault(),
                    SubjectSysId = c.Subject != null ? c.Subject.SubjectSysId : 0,

                    SubjectTitle = c.Subject != null ? c.Subject.CourseTitle : null,
                    SubjectDescription = c.Subject != null ? c.Subject.Description : null,
                    TitleImageUrl = c.Subject != null && !string.IsNullOrEmpty(c.Subject.TitleImagePath)
    ? $"/api/TrainingTitle/{c.Subject.SubjectSysId}/image"
    : null,

                    TitleImagePath = c.Subject != null ? c.Subject.TitleImagePath : null,

                    Cnecredits = c.Subject != null ? c.Subject.Cnecredits : false,
                    Oasascredits = c.Subject != null ? c.Subject.Oasascredits : false,
                    PeerCertCredits = c.Subject != null ? c.Subject.PeerCertCredits : false,
                    CreditHrs = c.Subject != null ? c.Subject.CreditHrs : null,

                    Sessions = _context.CourseSessions
                        .Where(s => s.CourseSysId == c.CourseSysId)
                        .Select(s => new
                        {
                            Date = s.SessionDate,
                            StartTime = s.StartTime,
                            EndTime = s.EndTime
                        }).ToList(),

                    FormatLabel = _context.LkFormats
                        .Where(f => f.Code == c.Format)
                        .Select(f => f.Value)
                        .FirstOrDefault(),

                    RegionLabel = _context.LkRegionCnties
                        .Where(r => r.Code == c.Region)
                        .Select(r => r.Value)
                        .FirstOrDefault(),

                    CategoryLabel = _context.LkCategories
                        .Where(cat => cat.Code == c.ContractType)
                        .Select(cat => cat.Value)
                        .FirstOrDefault(),

                    InstructorLabel = _context.Instructors
                        .Where(i => i.InstructorSysId == c.Instructor1)
                        .Select(i => i.Name)
                        .FirstOrDefault(),

                    InstructorNote = _context.Instructors
                        .Where(i => i.InstructorSysId == c.Instructor1)
                        .Select(i => i.InsNotes)
                        .FirstOrDefault(),

                    Instructor2Label = _context.Instructors
                        .Where(i => i.InstructorSysId == c.Instructor2)
                        .Select(i => i.Name)
                        .FirstOrDefault(),

                    Instructor2Note = _context.Instructors
                        .Where(i => i.InstructorSysId == c.Instructor2)
                        .Select(i => i.InsNotes)
                        .FirstOrDefault(),
                })
                .ToListAsync();

            return Ok(new { total, data });
        }

        private Task<int> GetRegisteredCountAsync(int courseId) =>
             _context.UserCourses.CountAsync(uc =>
                 uc.CourseSysId == courseId &&
                 uc.Status == 1 &&
                 !uc.IsWaitlisted);

        private async Task<int> GetNextWaitlistNumberAsync(int courseId)
        {
            var max = await _context.UserCourses
                .Where(uc => uc.CourseSysId == courseId && uc.IsWaitlisted)
                .MaxAsync(uc => (int?)uc.WaitlistNumber) ?? 0;
            return max + 1;
        }

        // helpers
        private static double? ParseDoubleInvariant(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (double.TryParse(s.Trim(), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d))
                return d;
            return null;
        }

        private static int ClampPct(double v)
        {
            if (v < 0) v = 0;
            if (v > 100) v = 100;
            return (int)Math.Round(v);
        }

        private static int ComputeProgressPercent(
    string? progressMeasure,
    string? completionStatus2004,
    string? lessonStatus12,
    string? successStatus2004,
    string? scoreRaw,
    string? lessonLocation,
    string? suspendData
)
        {
            string Norm(string? x) => (x ?? "").Trim().ToLowerInvariant();

            var ls = Norm(lessonStatus12);
            var cs = Norm(completionStatus2004);
            var ss = Norm(successStatus2004);

            if (ls is "completed" or "passed" or "failed") return 100;
            if (cs == "completed") return 100;
            if (ss is "passed" or "failed") return 100;

            var pm = ParseDoubleInvariant(progressMeasure);
            if (pm is not null)
            {
                var val = pm.Value;
                if (val <= 1.0) return ClampPct(val * 100.0);
                return ClampPct(val);
            }

            var sr = ParseDoubleInvariant(scoreRaw);
            if (sr is not null) return ClampPct(sr.Value);

            // ✅ Try numeric lesson_location (your fallback sometimes stores 9/100)
            var ll = ParseDoubleInvariant(lessonLocation);
            if (ll is not null) return ClampPct(ll.Value);

            // ✅ Try {"pct":9} from suspend_data
            if (!string.IsNullOrWhiteSpace(suspendData))
            {
                try
                {
                    using var doc = JsonDocument.Parse(suspendData);
                    if (doc.RootElement.TryGetProperty("pct", out var pctEl) && pctEl.TryGetInt32(out var pct))
                        return ClampPct(pct);
                }
                catch { /* ignore bad JSON */ }
            }

            return 0;
        }

        // Promote from waitlist if seats are available
        private async Task PromoteFromWaitlistAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null || !course.MaxSeats.HasValue || course.MaxSeats.Value <= 0)
                return;

            var registered = await GetRegisteredCountAsync(courseId);
            var seatsAvailable = course.MaxSeats.Value - registered;
            if (seatsAvailable <= 0) return;

            var toPromote = await _context.UserCourses
                .Where(uc => uc.CourseSysId == courseId && uc.Status == 1 && uc.IsWaitlisted)
                .OrderBy(uc => uc.WaitlistNumber)
                .Take(seatsAvailable)
                .ToListAsync();

            foreach (var uc in toPromote)
            {
                uc.IsWaitlisted = false;
                uc.WaitlistNumber = null;
                uc.DateStatusChanged = DateTime.UtcNow;
                uc.DateModified = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        [HttpGet("certificate/{courseId}")]
        public async Task<IActionResult> GetCertificate(int courseId, [FromQuery] Guid userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                    return NotFound("User not found.");

                var data = await (
                    from uc in _context.UserCourses
                    join c in _context.Courses on uc.CourseSysId equals c.CourseSysId
                    join s in _context.Subjects on c.SubjectSysId equals s.SubjectSysId into subjJoin
                    from subject in subjJoin.DefaultIfEmpty()
                    where uc.UserSysId == user.UserSysId && uc.CourseSysId == courseId
                    select new
                    {
                        uc.Status,
                        uc.Attended,
                        c.CourseDate,
                        c.EndDate,
                        c.Cancelled,
                        SubjectTitle = subject != null ? subject.CourseTitle : "Training Course"
                    }
                ).FirstOrDefaultAsync();

                if (data == null)
                    return NotFound("Course not found.");

                var eligible = data.Attended == true || data.Status == 3;
                if (!eligible || data.Cancelled == true)
                    return BadRequest("Certificate not available.");

                var templatePath = Path.Combine(_env.ContentRootPath, "Assets", "Certificates", "template.jpg");

                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest(new
                    {
                        message = "Template not found",
                        path = templatePath
                    });
                }

                var userName = $"{user.FirstName} {user.LastName}".Trim();
                var courseName = data.SubjectTitle ?? "Training Course";
                var completionDate = (data.EndDate ?? data.CourseDate ?? DateTime.Today).ToString("M/d/yyyy", CultureInfo.InvariantCulture);
                var trainingCenter = "AIDS Institute";

                QuestPDF.Settings.License = LicenseType.Community;

                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(0);

                        page.Content().Layers(layers =>
                        {
                            layers.PrimaryLayer()
                                .Image(templatePath, ImageScaling.FitArea);

                            layers.Layer()
                                .TranslateY(155)
                                .Width(PageSizes.A4.Landscape().Width)
                                .AlignCenter()
                                .Text($"This document acknowledges that {userName} has completed the following course:")
                                .FontSize(15)
                                .FontColor("#111111");

                            layers.Layer()
                                .TranslateY(200)
                                .Width(PageSizes.A4.Landscape().Width)
                                .AlignCenter()
                                .Text(courseName)
                                .FontSize(20)
                                .SemiBold()
                                .FontColor("#111111");

                            layers.Layer()
                                .TranslateY(280)
                                .Width(PageSizes.A4.Landscape().Width)
                                .AlignCenter()
                                .Text(userName)
                                .FontSize(22)
                                .SemiBold()
                                .FontColor("#111111");

                            layers.Layer()
                                .TranslateX(435)
                                .TranslateY(455)
                                .Width(170)
                                .AlignCenter()
                                .Text(completionDate)
                                .FontSize(11)
                                .FontColor("#111111");

                            layers.Layer()
                                .TranslateX(435)
                                .TranslateY(510)
                                .Width(170)
                                .AlignCenter()
                                .Text(trainingCenter)
                                .FontSize(11)
                                .FontColor("#111111");
                        });
                    });
                }).GeneratePdf();
                var safeFileName = new string(courseName
                    .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
                    .ToArray());

                safeFileName = string.Join("_", safeFileName
                    .Split('_', StringSplitOptions.RemoveEmptyEntries));

                if (string.IsNullOrWhiteSpace(safeFileName))
                {
                    safeFileName = "Training_Course";
                }
                var isDownload = Request.Query.ContainsKey("download") &&
                 Request.Query["download"] == "true";

                if (isDownload)
                {
                    return File(pdfBytes, "application/pdf", $"Certificate_{safeFileName}.pdf");
                }

                Response.Headers["Content-Disposition"] =
                    $"inline; filename=\"Certificate_{safeFileName}.pdf\"";

                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to generate certificate",
                    detail = ex.Message
                });
            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterCourse([FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("userId", out var userIdEl))
                    return BadRequest(new { message = "Missing userId" });

                var userIdStr = userIdEl.GetString();
                if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userGuid))
                    return BadRequest(new { message = "Invalid userId" });

                // accept either "courseId" or "courseSysId"
                int courseId = 0;
                if (body.TryGetProperty("courseId", out var cidEl)) courseId = cidEl.GetInt32();
                if (courseId <= 0 && body.TryGetProperty("courseSysId", out var csidEl)) courseId = csidEl.GetInt32();
                if (courseId <= 0) return BadRequest(new { message = "Missing or invalid courseId" });

                bool adaNeed = body.TryGetProperty("adaneed", out var adaNeedProp) && adaNeedProp.GetBoolean();
                string? adaDetails = body.TryGetProperty("adadetails", out var adaDetailsProp) ? adaDetailsProp.GetString() : null;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userGuid);
                if (user == null) return NotFound(new { message = "User not found" });

                var strategy = _context.Database.CreateExecutionStrategy();
                object? responsePayload = null;

                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                    var course = await _context.Courses.FindAsync(courseId);
                    if (course == null)
                    {
                        responsePayload = NotFound(new { message = "Course not found" });
                        return;
                    }

                    // Prevent duplicate active registrations
                    var existingActive = await _context.UserCourses
                        .FirstOrDefaultAsync(uc => uc.UserSysId == user.UserSysId
                                                && uc.CourseSysId == courseId
                                                && uc.Status == 1);
                    if (existingActive != null)
                    {
                        responsePayload = Ok(new
                        {
                            message = "Already registered.",
                            waitlist = existingActive.IsWaitlisted,
                            number = existingActive.WaitlistNumber
                        });
                        return;
                    }

                    // Capacity -> waitlist
                    var hasCapacity = course.MaxSeats.HasValue && course.MaxSeats.Value > 0;
                    var goesOnWaitlist = true;
                    int? waitlistNumber = null;

                    if (hasCapacity)
                    {
                        var registeredCount = await GetRegisteredCountAsync(courseId);
                        goesOnWaitlist = registeredCount >= course.MaxSeats.Value;
                    }

                    if (goesOnWaitlist)
                        waitlistNumber = await GetNextWaitlistNumberAsync(courseId);

                    var userCourse = new UserCourse
                    {
                        UserSysId = user.UserSysId,
                        CourseSysId = courseId,
                        Status = 1,
                        DateEntered = DateTime.UtcNow,
                        DateStatusChanged = DateTime.UtcNow,
                        DateModified = DateTime.UtcNow,
                        Token = Guid.NewGuid(),
                        Adaneed = adaNeed,
                        Adadetails = adaNeed ? adaDetails : null,
                        IsWaitlisted = goesOnWaitlist,
                        WaitlistNumber = waitlistNumber
                    };

                    _context.UserCourses.Add(userCourse);

                    // sync ADA to profile
                    user.Adaneed = adaNeed;
                    user.Adadetails = adaNeed ? adaDetails : null;
                    user.DateModified = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    responsePayload = Ok(new
                    {
                        message = goesOnWaitlist ? "Added to waitlist." : "Registration successful.",
                        waitlist = goesOnWaitlist,
                        number = waitlistNumber
                    });
                });

                // The delegate sets responsePayload to an IActionResult or null if it already returned
                if (responsePayload is IActionResult result) return result;

                // Fallback (shouldn’t happen)
                return Ok(new { message = "Registration processed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Registration failed", detail = ex.Message });
            }
        }

        [HttpGet("user-ada")]
        public async Task<IActionResult> GetUserAda([FromQuery] Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                adaneed = user.Adaneed ?? false,
                adadetails = user.Adadetails
            });
        }
        [HttpGet("check-registered")]
        public async Task<IActionResult> CheckIfRegistered([FromQuery] Guid userId, [FromQuery] int courseId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return Ok(new { isRegistered = false, userAda = (object?)null, courseAda = (object?)null });

            // is the user registered for this course?
            var userCourse = await _context.UserCourses
                .Where(uc => uc.UserSysId == user.UserSysId && uc.CourseSysId == courseId && uc.Status == 1) // 1 = Registered
                .Select(uc => new
                {
                    uc.Adaneed,
                    uc.Adadetails
                })
                .FirstOrDefaultAsync();

            bool already = userCourse != null;

            // user's profile ADA (fallback)
            var userAda = new
            {
                adaneed = user.Adaneed ?? false,
                adadetails = user.Adadetails
            };

            // course-specific ADA (preferred if registered)
            var courseAda = userCourse == null ? null : new
            {
                adaneed = userCourse.Adaneed ?? false,
                adadetails = userCourse.Adadetails
            };

            return Ok(new { isRegistered = already, userAda, courseAda });
        }

        [HttpGet("user-courses/{userId}")]
        public async Task<IActionResult> GetUserCourses(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound(new { message = "User not found" });

            var validStatuses = new List<int> { 1, 2, 3, 4, 6 };

            var userCourses = await (
    from uc in _context.UserCourses
    join c in _context.Courses on uc.CourseSysId equals c.CourseSysId
    join s in _context.Subjects on c.SubjectSysId equals s.SubjectSysId into subjJoin
    from subject in subjJoin.DefaultIfEmpty()
    where uc.UserSysId == user.UserSysId
       && uc.Status.HasValue
       && validStatuses.Contains(uc.Status.Value)
    select new
    {
        uc.CourseSysId,
        uc.Status,
        uc.IsWaitlisted,
        uc.Attended,
        c.CourseDate,
        c.EndDate,
        c.CourseTime,
        c.MaxSeats,
        c.Format,
        c.Cancelled,            // add this
        c.CancellReason,        // optional, useful later
        VideoUrl = subject != null ? subject.VideoUrl : null,
        IsOnlineTraining = subject != null && subject.IsOnlineTraining,
        SubjectTitle = subject != null ? subject.CourseTitle : null,
        SubjectDescription = subject != null ? subject.Description : null,
        TitleImageUrl = subject != null && !string.IsNullOrEmpty(subject.TitleImagePath)
            ? $"/api/TrainingTitle/{subject.SubjectSysId}/image"
            : null
    }
).ToListAsync();

            var scormCourseIds = userCourses
                .Where(x => x.Format.HasValue && x.Format.Value == 2)
                .Select(x => x.CourseSysId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var lastSessions = await _context.ScormAiccSessions
                .Where(s => s.Userid == user.UserSysId && scormCourseIds.Contains(s.Scormid))
                .GroupBy(s => s.Scormid)
                .Select(g => g.OrderByDescending(x => x.Attempt)
                              .ThenByDescending(x => x.Timemodified)
                              .FirstOrDefault())
                .ToListAsync();

            var sessionByScormId = lastSessions
                .Where(s => s != null)
                .ToDictionary(s => s!.Scormid, s => s!);

            var attempts = lastSessions
                .Where(s => s != null)
                .Select(s => s!.Attempt)
                .Distinct()
                .ToList();

            var tracks = await _context.ScormScoesTracks
                .Where(t => t.Userid.HasValue && t.Userid.Value == user.UserSysId
                         && t.Scormid.HasValue && scormCourseIds.Contains(t.Scormid.Value)
                         && t.Attempt.HasValue && attempts.Contains(t.Attempt.Value)
                         && (t.Element == "cmi.progress_measure"
                          || t.Element == "cmi.completion_status"
                          || t.Element == "cmi.success_status"
                          || t.Element == "cmi.core.lesson_status"
                          || t.Element == "cmi.core.score.raw"
                          || t.Element == "cmi.core.lesson_location"
                          || t.Element == "cmi.suspend_data"))
                .ToListAsync();

            var trackLookup = tracks
                .Where(t => t.Scormid.HasValue && t.Attempt.HasValue && !string.IsNullOrWhiteSpace(t.Element))
                .GroupBy(t => new { Scormid = t.Scormid!.Value, Attempt = t.Attempt!.Value, Element = t.Element! })
                .ToDictionary(
                    g => (g.Key.Scormid, g.Key.Attempt, g.Key.Element),
                    g => g.OrderByDescending(x => x.Timemodified ?? 0).First().Value
                );

            var formatDict = await _context.LkFormats
                .AsNoTracking()
                .ToDictionaryAsync(f => f.Code, f => f.Value);

            var completedOnlineCourseIdsToMarkAttended = new List<int>();

            object CourseDto(dynamic x)
            {
                int progress = 0;
                bool hasSession = false;
                bool completed = false;
                bool successfulCompletion = false;
                string label = "Launch Course";

                if (x.Format == 2)
                {
                    ScormAiccSession? sess = null;
                    sessionByScormId.TryGetValue((int)x.CourseSysId, out sess);

                    if (sess != null)
                    {
                        hasSession = true;

                        int attempt = sess.Attempt ?? 0;

                        trackLookup.TryGetValue((sess.Scormid, attempt, "cmi.progress_measure"), out var pm);
                        trackLookup.TryGetValue((sess.Scormid, attempt, "cmi.completion_status"), out var cs);
                        trackLookup.TryGetValue((sess.Scormid, attempt, "cmi.core.lesson_status"), out var ls);
                        trackLookup.TryGetValue((sess.Scormid, attempt, "cmi.success_status"), out var ss);
                        trackLookup.TryGetValue((sess.Scormid, attempt, "cmi.core.score.raw"), out var sr);
                        trackLookup.TryGetValue((sess.Scormid, attempt, "cmi.core.lesson_location"), out var ll);
                        trackLookup.TryGetValue((sess.Scormid, attempt, "cmi.suspend_data"), out var sd);

                        progress = ComputeProgressPercent(pm, cs, ls, ss, sr, ll, sd);

                        var normalizedCompletionStatus = (cs ?? "").Trim().ToLowerInvariant();
                        var normalizedSuccessStatus = (ss ?? "").Trim().ToLowerInvariant();
                        var normalizedSessionScormStatus = (sess.Scormstatus ?? "").Trim().ToLowerInvariant();
                        var normalizedSessionLessonStatus = (sess.Lessonstatus ?? "").Trim().ToLowerInvariant();

                        successfulCompletion =
                            progress >= 100 &&
                            (
                                normalizedSessionScormStatus == "completed" ||
                                normalizedSessionLessonStatus == "completed" ||
                                normalizedSessionLessonStatus == "passed" ||
                                normalizedCompletionStatus == "completed" ||
                                normalizedSuccessStatus == "passed"
                            );

                        completed = successfulCompletion;

                        if (successfulCompletion)
                        {
                            progress = 100;
                            label = "Retake the course";
                            completedOnlineCourseIdsToMarkAttended.Add((int)x.CourseSysId);
                        }
                        else
                        {
                            label = progress > 0 ? "Resume Course" : "Launch Course";
                        }
                    }
                }

                int? fmt = x.Format as int?;
                string? formatLabel = null;

                if (fmt.HasValue && formatDict.TryGetValue(fmt.Value, out var lbl))
                    formatLabel = lbl;

                var courseDate = x.CourseDate as DateTime?;
                var endDate = x.EndDate as DateTime?;
                var effectiveCourseEnd = endDate ?? courseDate;
                var today = DateTime.Today;

                string learningSection = "inProgress";

                if (x.Cancelled == true || x.Status == 2)
                {
                    learningSection = "cancelled";
                }
                else if (x.Status == 6)
                {
                    learningSection = "dropped";
                }
                else if (x.Status == 4)
                {
                    learningSection = "absent";
                }
                else if (x.Format == 2)
                {
                    // Online / SCORM
                    if (x.Attended == true || x.Status == 3 || successfulCompletion)
                    {
                        learningSection = "attended";
                    }
                    else
                    {
                        learningSection = "inProgress";
                    }
                }
                else
                {
                    // Non-online formats
                    if (x.Attended == true || x.Status == 3)
                    {
                        learningSection = "attended";
                    }
                    else if (effectiveCourseEnd.HasValue && effectiveCourseEnd.Value.Date < today)
                    {
                        learningSection = "absent";
                    }
                    else
                    {
                        learningSection = "inProgress";
                    }
                }

                return new
                {
                    x.CourseSysId,
                    x.Status,
                    x.IsWaitlisted,
                    x.Attended,
                    x.Cancelled,
                    x.CancellReason,
                    x.CourseDate,
                    x.EndDate,
                    x.CourseTime,
                    x.MaxSeats,
                    x.Format,
                    FormatLabel = formatLabel,
                    x.VideoUrl,
                    x.IsOnlineTraining,
                    x.SubjectTitle,
                    x.SubjectDescription,
                    x.TitleImageUrl,

                    ScormProgress = progress,
                    ScormHasSession = hasSession,
                    ScormCompleted = completed,
                    ScormButtonLabel = label,

                    LearningSection = learningSection
                };
            }

            var previewResult = userCourses.Select(x => CourseDto(x)).ToList();

            if (completedOnlineCourseIdsToMarkAttended.Any())
            {
                var idsToUpdate = completedOnlineCourseIdsToMarkAttended.Distinct().ToList();

                var userCourseRowsToUpdate = await _context.UserCourses
                    .Where(uc =>
                        uc.UserSysId == user.UserSysId &&
                        idsToUpdate.Contains(uc.CourseSysId) &&
                        uc.Status == 1 &&
                        uc.Attended != true)
                    .ToListAsync();

                if (userCourseRowsToUpdate.Any())
                {
                    var now = DateTime.UtcNow;

                    foreach (var row in userCourseRowsToUpdate)
                    {
                        row.Attended = true;
                        row.Status = 3;
                        row.DateModified = now;
                        row.DateStatusChanged = now;
                    }

                    await _context.SaveChangesAsync();

                    // re-run once so frontend gets latest status/section immediately
                    return await GetUserCourses(userId);
                }
            }

            var result = userCourses.Select(x => CourseDto(x)).ToList();
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.CourseSysId == id);

            if (course == null)
            {
                return NotFound();
            }

            var result = new
            {
                course.CourseSysId,
                course.CourseDate,
                course.EndDate,          // ✅ add
                course.CourseTime,
                course.RegDeadLine,      // ✅ add
                course.Information,
                course.City,
                course.TrainingLocation,
                course.MaxSeats,
                IsMultiSession = course.IsMultiSession,
                course.VirtualUrl,

                TrainingUrl = course.Subject != null ? course.Subject.VideoUrl : null,  // ✅ add
                TitleImageUrl = course.Subject != null && !string.IsNullOrEmpty(course.Subject.TitleImagePath)
    ? $"/api/TrainingTitle/{course.Subject.SubjectSysId}/image"
    : null,

                SubjectTitle = course.Subject?.CourseTitle,
                SubjectDescription = course.Subject?.Description,
                Cnecredits = course.Subject != null ? course.Subject.Cnecredits : false,
                Oasascredits = course.Subject != null ? course.Subject.Oasascredits : false,
                PeerCertCredits = course.Subject != null ? course.Subject.PeerCertCredits : false,
                CreditHrs = course.Subject != null ? course.Subject.CreditHrs : null,

                FormatLabel = _context.LkFormats
                    .Where(f => f.Code == course.Format)
                    .Select(f => f.Value)
                    .FirstOrDefault(),

                RegionLabel = _context.LkRegionCnties
                    .Where(r => r.Code == course.Region)
                    .Select(r => r.Value)
                    .FirstOrDefault(),

                CategoryLabel = _context.LkCategories
                    .Where(cat => cat.Code == course.ContractType)
                    .Select(cat => cat.Value)
                    .FirstOrDefault(),

                SiteName = _context.Sites
                    .Where(s => s.SiteSysId == course.SiteSysId)
                    .Select(s => s.SiteName)
                    .FirstOrDefault(),

                InstructorLabel = _context.Instructors
                    .Where(i => i.InstructorSysId == course.Instructor1)
                    .Select(i => i.Name)
                    .FirstOrDefault(),

                InstructorNote = _context.Instructors
                    .Where(i => i.InstructorSysId == course.Instructor1)
                    .Select(i => i.InsNotes)
                    .FirstOrDefault(),

                Instructor2Label = _context.Instructors
                    .Where(i => i.InstructorSysId == course.Instructor2)
                    .Select(i => i.Name)
                    .FirstOrDefault(),

                Instructor2Note = _context.Instructors
                    .Where(i => i.InstructorSysId == course.Instructor2)
                    .Select(i => i.InsNotes)
                    .FirstOrDefault(),

                Sessions = await _context.CourseSessions
  .Where(s => s.CourseSysId == course.CourseSysId)
  .Select(s => new {
      SessionDate = s.SessionDate,
      StartTime = s.StartTime,
      EndTime = s.EndTime,
      SessionUrl = s.SessionUrl,
      TrainingLocation = s.TrainingLocation
  })
  .ToListAsync()

            };

            return Ok(result);
        }


        [HttpPost("drop")]
        public async Task<IActionResult> DropCourse([FromBody] JsonElement body)
        {
            try
            {
                if (!body.TryGetProperty("userId", out var userIdEl))
                    return BadRequest(new { message = "Missing userId" });

                var userGuid = Guid.Parse(userIdEl.GetString() ?? string.Empty);

                int courseId = 0;
                if (body.TryGetProperty("courseId", out var cidEl)) courseId = cidEl.GetInt32();
                if (courseId <= 0 && body.TryGetProperty("courseSysId", out var csidEl)) courseId = csidEl.GetInt32();
                if (courseId <= 0) return BadRequest(new { message = "Missing or invalid courseId" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userGuid);
                if (user == null) return NotFound(new { message = "User not found" });

                var strategy = _context.Database.CreateExecutionStrategy();
                object? responsePayload = null;

                await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                    var userCourse = await _context.UserCourses
                        .FirstOrDefaultAsync(uc => uc.UserSysId == user.UserSysId && uc.CourseSysId == courseId && uc.Status == 1);

                    if (userCourse == null)
                    {
                        responsePayload = NotFound(new { message = "Registration not found or already dropped." });
                        return;
                    }

                    userCourse.Status = 6; // Dropped
                    userCourse.DateStatusChanged = DateTime.UtcNow;
                    userCourse.DateModified = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // try to promote from waitlist now that a seat freed up
                    await PromoteFromWaitlistAsync(courseId);

                    await tx.CommitAsync();

                    responsePayload = Ok(new { message = "Course dropped successfully." });
                });

                if (responsePayload is IActionResult result) return result;

                return Ok(new { message = "Drop processed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Drop failed", detail = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCourses(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 9,
    [FromQuery] string? search = null)
        {
            // Step 1: Base query
            var query = _context.Courses
                .Where(c => !c.Hidden)
                .AsQueryable(); // Ensures it stays IQueryable

            // Step 2: Apply search filter if needed
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    (c.Subject != null && EF.Functions.Like(c.Subject.CourseTitle, $"%{search}%")) ||
                    (c.Subject != null && EF.Functions.Like(c.Subject.Description, $"%{search}%")) ||
                    (c.City != null && EF.Functions.Like(c.City, $"%{search}%")) ||
                    (c.Information != null && EF.Functions.Like(c.Information, $"%{search}%"))
                );
            }

            // Step 3: Apply Include separately after filtering
            query = query.Include(c => c.Subject);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(c => c.CourseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.CourseSysId,
                    c.CourseDate,
                    c.CourseTime,
                    c.Information,
                    c.City,
                    c.TrainingLocation,
                    c.MaxSeats,
                    c.Format,
                    SubjectTitle = c.Subject != null ? c.Subject.CourseTitle : "N/A",
                    SubjectDescription = c.Subject != null ? c.Subject.Description : "N/A"
                })
                .ToListAsync();

            return Ok(new { total, data });
        }
    }
}

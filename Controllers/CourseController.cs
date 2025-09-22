// using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HIVTraining.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseController(ApplicationDbContext context)
        {
            _context = context;
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

                    SubjectTitle = c.Subject.CourseTitle,
                    SubjectDescription = c.Subject.Description,
                    c.Subject.Cnecredits,
                    c.Subject.Oasascredits,
                    c.Subject.PeerCertCredits,
                    c.Subject.CreditHrs,

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


        [HttpPost("register")]
        public async Task<IActionResult> RegisterCourse([FromBody] JsonElement body)
        {
            var userId = body.GetProperty("userId").GetGuid();
            var courseId = body.GetProperty("courseId").GetInt32();

            bool adaNeed = body.TryGetProperty("adaneed", out var adaNeedProp) && adaNeedProp.GetBoolean();
            string? adaDetails = body.TryGetProperty("adadetails", out var adaDetailsProp) ? adaDetailsProp.GetString() : null;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseSysId == courseId);
            if (course == null)
                return NotFound(new { message = "Course not found" });

            bool isWaitlisted = course.MaxSeats == null || course.MaxSeats <= 0;
            int? waitlistNumber = null;

            if (isWaitlisted)
            {
                waitlistNumber = await _context.UserCourses
                    .Where(uc => uc.CourseSysId == courseId && uc.IsWaitlisted)
                    .CountAsync() + 1;
            }

            var userCourse = new UserCourse
            {
                UserSysId = user.UserSysId,
                CourseSysId = courseId,
                Status = 1,
                DateEntered = DateTime.UtcNow,
                DateStatusChanged = DateTime.UtcNow,
                Token = Guid.NewGuid(),
                Adaneed = adaNeed,
                Adadetails = adaNeed ? adaDetails : null,
                IsWaitlisted = isWaitlisted,
                WaitlistNumber = waitlistNumber
            };

            _context.UserCourses.Add(userCourse);

            // ✅ Always sync ADA on the user profile to reflect latest choice
            user.Adaneed = adaNeed;
            user.Adadetails = adaNeed ? adaDetails : null;
            user.DateModified = DateTime.UtcNow;

            // Optional but explicit: mark as modified to avoid any tracking surprises
            _context.Users.Update(user);

            if (!isWaitlisted && course.MaxSeats.HasValue)
                course.MaxSeats--;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = isWaitlisted ? "Registered to waitlist." : "Registration successful.",
                userAda = new { adaneed = user.Adaneed ?? false, adadetails = user.Adadetails }
            });
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
            if (user == null)
                return NotFound(new { message = "User not found" });

            var validStatuses = new List<int> { 1, 2, 3, 4 }; // Registered, Cancelled, Attended, Absent

            var userCourses = await (
                from uc in _context.UserCourses
                join c in _context.Courses on uc.CourseSysId equals c.CourseSysId
                join s in _context.Subjects on c.SubjectSysId equals s.SubjectSysId into subjJoin
                from subject in subjJoin.DefaultIfEmpty()

                where uc.UserSysId == user.UserSysId && uc.Status.HasValue && validStatuses.Contains(uc.Status.Value)
                select new
                {
                    uc.CourseSysId,
                    uc.Status,
                    uc.IsWaitlisted, 
                    c.CourseDate,
                    c.CourseTime,
                    c.MaxSeats,
                    SubjectTitle = subject.CourseTitle,
                    SubjectDescription = subject.Description
                }
            ).ToListAsync();

            return Ok(userCourses);
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
                course.CourseTime,
                course.Information,
                course.City,
                course.TrainingLocation,
                course.MaxSeats,
                IsMultiSession = course.IsMultiSession,

                SubjectTitle = course.Subject?.CourseTitle,
                SubjectDescription = course.Subject?.Description,
                course.Subject?.Cnecredits,
                course.Subject?.Oasascredits,
                course.Subject?.PeerCertCredits,
                course.Subject?.CreditHrs,

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
                    .Select(s => new
                    {
                        Date = s.SessionDate,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime
                    }).ToListAsync()
            };

            return Ok(result);
        }


        [HttpPost("drop")]
        public async Task<IActionResult> DropCourse([FromBody] JsonElement body)
        {
            var userId = body.GetProperty("userId").GetGuid();
            var courseId = body.GetProperty("courseId").GetInt32();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseSysId == courseId);
            if (course == null)
                return NotFound(new { message = "Course not found" });

            var userCourse = await _context.UserCourses.FirstOrDefaultAsync(uc => uc.UserSysId == user.UserSysId && uc.CourseSysId == courseId && uc.Status == 1);
            if (userCourse == null)
                return NotFound(new { message = "Registration not found or already dropped." });

            userCourse.Status = 6; // 6 = Dropped
            userCourse.DateStatusChanged = DateTime.UtcNow;

            // Increase the seats
            if (course.MaxSeats.HasValue)
                course.MaxSeats++;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Course dropped successfully." });
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

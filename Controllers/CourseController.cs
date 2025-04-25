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
    int format,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 9,
    [FromQuery] string? search = null,
    [FromQuery] int? region = null,
    [FromQuery] int? category = null,
    [FromQuery] int? site = null,
    [FromQuery] DateTime? fromDate = null,
    [FromQuery] DateTime? toDate = null)
{
    var baseQuery = _context.Courses
        .Where(c => !c.Hidden);

    // Apply format filter (0 means 'All')
    if (format != 0)
    {
        baseQuery = baseQuery.Where(c => c.Format == format);
    }

    // Apply search filter
    if (!string.IsNullOrWhiteSpace(search))
    {
        baseQuery = baseQuery.Where(c =>
            (c.Subject != null && EF.Functions.Like(c.Subject.CourseTitle, $"%{search}%")) ||
            (c.Subject != null && EF.Functions.Like(c.Subject.Description, $"%{search}%")) ||
            (c.City != null && EF.Functions.Like(c.City, $"%{search}%")) ||
            (c.Information != null && EF.Functions.Like(c.Information, $"%{search}%"))
        );
    }

    // Apply advanced filters
    if (region.HasValue)
    {
        baseQuery = baseQuery.Where(c => c.Region == region);
    }

    if (category.HasValue)
    {
        baseQuery = baseQuery.Where(c => c.ContractType == category);
    }

    if (site.HasValue)
    {
        baseQuery = baseQuery.Where(c => c.SiteSysId == site);
    }

    if (fromDate.HasValue)
    {
        baseQuery = baseQuery.Where(c => c.CourseDate >= fromDate.Value);
    }

    if (toDate.HasValue)
    {
        baseQuery = baseQuery.Where(c => c.CourseDate <= toDate.Value);
    }

    // Include Subject
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

            Console.WriteLine($"Registering CourseId: {courseId} for UserId: {userId} ADA: {adaNeed}, Details: {adaDetails}");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseSysId == courseId);
            if (course == null)
                return NotFound(new { message = "Course not found" });

            // ✅ Check if seats are available
            if (course.MaxSeats.HasValue && course.MaxSeats <= 0)
                return BadRequest(new { message = "No seats available for this course." });

            // ✅ Create user-course entry
            var userCourse = new UserCourse
            {
                UserSysId = user.UserSysId,
                CourseSysId = courseId,
                Status = 1,
                DateEntered = DateTime.UtcNow,
                DateStatusChanged = DateTime.UtcNow,
                Token = Guid.NewGuid(),
                Adaneed = adaNeed,
                Adadetails = adaNeed ? adaDetails : null
            };

            _context.UserCourses.Add(userCourse);

            // ✅ Reduce available seats
            if (course.MaxSeats.HasValue)
                course.MaxSeats--;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful." });
        }

        [HttpGet("check-registered")]
        public async Task<IActionResult> CheckIfRegistered([FromQuery] Guid userId, [FromQuery] int courseId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return Ok(new { isRegistered = false });

            var already = await _context.UserCourses.AnyAsync(uc =>
                uc.UserSysId == user.UserSysId && uc.CourseSysId == courseId && uc.Status == 1); // 1 = Registered

            return Ok(new { isRegistered = already });
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

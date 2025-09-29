// Updated CourseAdminController with region, format, category, and site filtering
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using HIVTraining_Vue.Server.Models;
using HIVTraining_Vue.Server.Requests;

namespace HIVTraining_Vue.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CourseAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedCourses(
    int page = 1,
    int pageSize = 100,
    string? title = null,
    int? siteId = null,
    int? region = null,
    int? format = null,
    int? category = null,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    bool onlyActive = false
)
        {
            try
            {
                var query =
                    from c in _context.Courses
                    join s in _context.Sites on c.SiteSysId equals s.SiteSysId into siteJoin
                    from s in siteJoin.DefaultIfEmpty()

                    join subj in _context.Subjects on c.SubjectSysId equals subj.SubjectSysId into subjectJoin
                    from subj in subjectJoin.DefaultIfEmpty()

                    join i1 in _context.Users on c.Instructor1 equals i1.UserSysId into i1Join
                    from i1 in i1Join.DefaultIfEmpty()

                    join i2 in _context.Users on c.Instructor2 equals i2.UserSysId into i2Join
                    from i2 in i2Join.DefaultIfEmpty()

                    let regionName = (from r in _context.LkRegionCnties
                                      where r.Code == c.Region
                                      select r.Value).FirstOrDefault()

                    let categoryName = (from cat in _context.LkCategories
                                        where cat.Code == c.ContractType
                                        select cat.Value).FirstOrDefault()

                    // computed badges
                    let hasWaitlist = _context.UserCourses.Any(uc =>
                        uc.CourseSysId == c.CourseSysId &&
                        uc.Status == 1 &&                // registered
                        uc.IsWaitlisted                  // is on waitlist
                    )

                    let hasAda = _context.UserCourses.Any(uc =>
                        uc.CourseSysId == c.CourseSysId &&
                        uc.Status == 1 &&                // registered
                        (
                            (uc.Adaneed ?? false) ||     // nullable bool
                            !string.IsNullOrEmpty(uc.Adadetails)
                        )
                    )

                    select new
                    {
                        c.CourseSysId,
                        c.SubjectSysId,
                        c.SiteSysId,
                        c.Format,
                        c.ContractType,
                        c.Instructor1,
                        c.Instructor2,
                        c.RegDeadLine,
                        c.MaxSeats,
                        c.CourseDate,
                        c.EndDate,
                        c.CourseTimeBegin,
                        c.CourseTimeEnd,
                        c.TrainingLocation,
                        c.Deliverable,
                        c.Information,
                        c.Rtc,
                        c.Coe,
                        c.OtherFund,
                        c.Hidden,
                        c.Delivered,
                        c.Approve,
                        c.Cancelled,

                        SubjectTitle = subj.CourseTitle ?? "N/A",
                        SiteName = s.SiteName ?? "N/A",
                        RegionLabel = regionName ?? "N/A",
                        CategoryLabel = categoryName ?? "N/A",

                        RegisteredCount = _context.UserCourses.Count(uc =>
    uc.CourseSysId == c.CourseSysId && uc.Status == 1 && !uc.IsWaitlisted),
                        WaitlistCount = _context.UserCourses.Count(uc =>
                            uc.CourseSysId == c.CourseSysId && uc.Status == 1 && uc.IsWaitlisted),
                        // optionally:
                        TotalRegistrations = _context.UserCourses.Count(uc =>
                            uc.CourseSysId == c.CourseSysId && uc.Status == 1),

                        InstructorLabel = (i1.FirstName + " " + (i1.Mi ?? "") + " " + i1.LastName).Trim(),
                        Instructor2Label = (i2.FirstName + " " + (i2.Mi ?? "") + " " + i2.LastName).Trim(),

                        HasWaitlist = hasWaitlist,
                        HasAda = hasAda
                    };

                if (onlyActive)
                    query = query.Where(c => c.Cancelled == false || c.Cancelled == null);

                if (!string.IsNullOrWhiteSpace(title))
                    query = query.Where(c => c.SubjectTitle.Contains(title));

                if (siteId.HasValue)
                    query = query.Where(c => c.SiteSysId == siteId.Value);

                if (region.HasValue)
                    query = query.Where(c => c.RegionLabel != null && c.RegionLabel != "N/A" &&
                                             (from r in _context.LkRegionCnties
                                              where r.Code == region
                                              select r.Value).FirstOrDefault() == c.RegionLabel);

                if (format.HasValue)
                    query = query.Where(c => c.Format == format.Value);

                if (category.HasValue)
                    query = query.Where(c => c.ContractType == category.Value);

                if (fromDate.HasValue)
                    query = query.Where(c => c.CourseDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(c => c.CourseDate <= toDate.Value);

                var total = await query.CountAsync();

                var pagedCourses = await query
                    .OrderBy(c => c.Cancelled ?? false)   // active first
                    .ThenByDescending(c => c.CourseDate)  // newest first
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new { data = pagedCourses, total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("counts")]
        public async Task<IActionResult> GetCourseCounts([FromQuery] int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var enrolled = await _context.UserCourses.CountAsync(uc =>
                uc.CourseSysId == courseId && uc.Status == 1 && !uc.IsWaitlisted);

            var waitlist = await _context.UserCourses.CountAsync(uc =>
                uc.CourseSysId == courseId && uc.Status == 1 && uc.IsWaitlisted);

            return Ok(new
            {
                courseId,
                enrolledCount = enrolled,            // non-waitlisted
                waitlistCount = waitlist,            // waitlisted
                totalRegistrations = enrolled + waitlist,
                maxSeats = course.MaxSeats,
                hasWaitlist = waitlist > 0
            });
        }
        [HttpGet("ada-registrations")]
        public async Task<IActionResult> GetAdaRegistrations([FromQuery] int courseId)
        {
            var query = from uc in _context.UserCourses
                        join u in _context.Users on uc.UserSysId equals u.UserSysId
                        where uc.CourseSysId == courseId
                           && uc.Status == 1                       // registered
                           && ((uc.Adaneed ?? false) || !string.IsNullOrEmpty(uc.Adadetails))
                        orderby u.LastName, u.FirstName
                        select new
                        {
                            userSysId = u.UserSysId,
                            fullName = (u.FirstName + " " + (u.Mi ?? "") + " " + u.LastName).Trim(),
                            email = u.Email,
                            adaNeed = uc.Adaneed ?? false,
                            adaDetails = uc.Adadetails
                        };

            var data = await query.ToListAsync();
            return Ok(data);
        }

        [HttpGet("courseWithSessions/{id}")]
        public async Task<IActionResult> GetCourseWithSessions(int id)
        {
            var c = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CourseSysId == id);

            if (c == null) return NotFound(new { message = "Course not found" });

            var sessions = await _context.CourseSessions
                .Where(s => s.CourseSysId == id)
                .OrderBy(s => s.SessionDate)
                .Select(s => new
                {
                    sessionDate = s.SessionDate,     // string/ISO is fine; frontend does split("T")[0]
                    startTime = s.StartTime,       // frontend uses substring(0,5)
                    endTime = s.EndTime,
                    sessionUrl = s.SessionUrl
                })
                .ToListAsync();

            // shape matches what your modal’s populateForm() reads
            return Ok(new
            {
                c.CourseSysId,
                c.SiteSysId,
                c.Region,
                c.SubjectSysId,
                c.Instructor1,
                c.Instructor2,
                c.CourseDate,
                c.EndDate,
                c.CourseTimeBegin,
                c.CourseTimeEnd,
                c.RegDeadLine,
                c.MaxSeats,
                c.TrainingLocation,
                c.Deliverable,
                c.Format,
                c.Rtc,
                c.Coe,
                c.OtherFund,
                c.Hidden,
                c.Information,
                isMultiSession = c.IsMultiSession,  // your Vue reads c.isMultiSession
                sessions                        // your Vue handles .sessions or .sessions.$values
            });
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseScheduleRequest request)
        {
            if (request == null || request.Course == null)
                return BadRequest("Course data is required.");

            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null) return NotFound();

            var updated = request.Course;

            // Update main course fields
            existingCourse.SiteSysId = updated.SiteSysId;
            existingCourse.SubjectSysId = updated.SubjectSysId;
            existingCourse.Region = updated.Region;
            existingCourse.Instructor1 = updated.Instructor1;
            existingCourse.Instructor2 = updated.Instructor2;
            existingCourse.CourseDate = updated.CourseDate;
            existingCourse.EndDate = updated.EndDate;
            existingCourse.CourseTimeBegin = updated.CourseTimeBegin;
            existingCourse.CourseTimeEnd = updated.CourseTimeEnd;
            existingCourse.RegDeadLine = updated.RegDeadLine;
            existingCourse.MaxSeats = updated.MaxSeats;
            existingCourse.TrainingLocation = updated.TrainingLocation;
            existingCourse.Deliverable = updated.Deliverable;
            existingCourse.Format = updated.Format;
            existingCourse.Rtc = updated.Rtc;
            existingCourse.Coe = updated.Coe;
            existingCourse.OtherFund = updated.OtherFund;
            existingCourse.Hidden = updated.Hidden;
            existingCourse.Information = updated.Information;
            existingCourse.IsMultiSession = updated.IsMultiSession;
            existingCourse.DateModified = DateTime.UtcNow;

            // Clear existing sessions
            var existingSessions = await _context.CourseSessions
                .Where(s => s.CourseSysId == id)
                .ToListAsync();

            _context.CourseSessions.RemoveRange(existingSessions);

            // Add new sessions if applicable
            if (updated.IsMultiSession && request.Sessions != null && request.Sessions.Any())
            {
                var newSessions = request.Sessions.Select(s => new CourseSession
                {
                    CourseSysId = id,
                    SessionDate = s.SessionDate,
                    StartTime = TimeSpan.Parse(s.StartTime),
                    EndTime = TimeSpan.Parse(s.EndTime),
                    SessionUrl = s.SessionUrl
                }).ToList();

                _context.CourseSessions.AddRange(newSessions);
            }

            await _context.SaveChangesAsync();

            //  Handle waitlisted users if seats are available
            if (existingCourse.MaxSeats.HasValue && existingCourse.MaxSeats.Value > 0)
            {
                var registered = await _context.UserCourses.CountAsync(uc =>
                    uc.CourseSysId == id && uc.Status == 1 && !uc.IsWaitlisted);

                var seatsAvailable = existingCourse.MaxSeats.Value - registered;

                if (seatsAvailable > 0)
                {
                    var toPromote = await _context.UserCourses
                        .Where(uc => uc.CourseSysId == id && uc.Status == 1 && uc.IsWaitlisted)
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
            }

            return Ok(new { message = "Course updated successfully!" });
        }

        [HttpGet("search-users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? lastName = null, [FromQuery] string? email = null)
        {
            var query = from u in _context.Users
                        join r in _context.AspnetRoles on u.Role equals r.RoleId into roleJoin
                        from r in roleJoin.DefaultIfEmpty()
                        where u.Active
                        select new
                        {
                            u.UserSysId,
                            FullName = (u.FirstName + " " + (u.Mi ?? "") + " " + u.LastName).Trim(),
                            u.Email,
                            Role = r != null ? r.RoleName : "N/A"
                        };

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(u => EF.Functions.Like(u.FullName, $"%{lastName}%"));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(u => EF.Functions.Like(u.Email, $"%{email}%")); 

            var users = await query.Take(50).ToListAsync();
            return Ok(users);
        }

        [HttpPost("add-user-to-course")]
        public async Task<IActionResult> AddUserToCourse([FromBody] UserCourse request)
        {
            if (request == null || request.UserSysId == 0 || request.CourseSysId == 0)
                return BadRequest("Invalid data.");

            var course = await _context.Courses.FindAsync(request.CourseSysId);
            if (course == null) return NotFound("Course not found.");

            // ✅ Reconcile: if seats opened, promote from waitlist first
            var _ = await PromoteFromWaitlistAsync(request.CourseSysId);

            // Recompute capacity after reconciliation
            bool hasCapacity = course.MaxSeats.HasValue && course.MaxSeats.Value > 0;
            int registeredCount = await _context.UserCourses.CountAsync(uc =>
                uc.CourseSysId == request.CourseSysId && uc.Status == 1 && !uc.IsWaitlisted);

            bool goesOnWaitlist = !hasCapacity || registeredCount >= course.MaxSeats!.Value;

            int? waitlistNumber = null;
            if (goesOnWaitlist)
            {
                waitlistNumber = await _context.UserCourses
                    .Where(uc => uc.CourseSysId == request.CourseSysId && uc.IsWaitlisted)
                    .MaxAsync(uc => (int?)uc.WaitlistNumber) ?? 0;
                waitlistNumber += 1;
            }

            var existing = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserSysId == request.UserSysId &&
                                           uc.CourseSysId == request.CourseSysId);

            if (existing != null)
            {
                existing.Status = 1;
                existing.IsWaitlisted = goesOnWaitlist;
                existing.WaitlistNumber = goesOnWaitlist ? waitlistNumber : null;
                existing.DateModified = DateTime.UtcNow;
                existing.DateStatusChanged = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.UserCourses.Add(new UserCourse
                {
                    UserSysId = request.UserSysId,
                    CourseSysId = request.CourseSysId,
                    Status = 1,
                    DateEntered = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                    DateStatusChanged = DateTime.UtcNow,
                    Token = Guid.NewGuid(),
                    IsWaitlisted = goesOnWaitlist,
                    WaitlistNumber = waitlistNumber
                });
                await _context.SaveChangesAsync();
            }

            // Return fresh counts so the UI can reconcile precisely
            var registered = await _context.UserCourses.CountAsync(uc =>
                uc.CourseSysId == request.CourseSysId && uc.Status == 1 && !uc.IsWaitlisted);
            var hasWaitlistNow = await _context.UserCourses.AnyAsync(uc =>
                uc.CourseSysId == request.CourseSysId && uc.Status == 1 && uc.IsWaitlisted);

            return Ok(new
            {
                waitlist = goesOnWaitlist,
                number = waitlistNumber,
                counts = new { registeredCount = registered, maxSeats = course.MaxSeats, hasWaitlist = hasWaitlistNow }
            });
        }

        [HttpGet("registered-user-status")]
        public async Task<IActionResult> GetRegisteredUserStatus([FromQuery] int courseId)
        {
            var registrations = await _context.UserCourses
                .Where(uc => uc.CourseSysId == courseId && (uc.Status == 1 || uc.Status == 6))
                .Select(uc => new
                {
                    uc.UserSysId,
                    uc.Status
                })
                .ToListAsync();

            return Ok(registrations);
        }



        [HttpGet("registered-user-ids")]
        public async Task<IActionResult> GetRegisteredUserIds([FromQuery] int courseId)
        {
            var userIds = await _context.UserCourses
                .Where(uc => uc.CourseSysId == courseId && uc.Status == 1) // Registered only
                .Select(uc => uc.UserSysId)
                .ToListAsync();

            return Ok(userIds);
        }
        // 🔹 Helper
        private async Task<(bool promoted, bool waitlistStillExists)> PromoteFromWaitlistAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return (false, false);

            if (!(course.MaxSeats.HasValue && course.MaxSeats.Value > 0))
            {
                // no capacity defined => nothing to promote
                var stillHasWaitlist = await _context.UserCourses
                    .AnyAsync(uc => uc.CourseSysId == courseId && uc.Status == 1 && uc.IsWaitlisted);
                return (false, stillHasWaitlist);
            }

            var registeredCount = await _context.UserCourses.CountAsync(uc =>
                uc.CourseSysId == courseId && uc.Status == 1 && !uc.IsWaitlisted);

            var seatsAvailable = course.MaxSeats.Value - registeredCount;
            if (seatsAvailable <= 0)
            {
                var stillHasWaitlist = await _context.UserCourses
                    .AnyAsync(uc => uc.CourseSysId == courseId && uc.Status == 1 && uc.IsWaitlisted);
                return (false, stillHasWaitlist);
            }

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

            if (toPromote.Count > 0)
                await _context.SaveChangesAsync();

            var waitlistStillExists = await _context.UserCourses
                .AnyAsync(uc => uc.CourseSysId == courseId && uc.Status == 1 && uc.IsWaitlisted);

            return (toPromote.Count > 0, waitlistStillExists);
        }

        [HttpPut("drop-user")]
        public async Task<IActionResult> DropUserFromCourse([FromBody] UserCourse request)
        {
            if (request == null || request.UserSysId == 0 || request.CourseSysId == 0)
                return BadRequest("Invalid request.");

            var userCourse = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserSysId == request.UserSysId &&
                                           uc.CourseSysId == request.CourseSysId &&
                                           uc.Status == 1);
            if (userCourse == null)
                return NotFound("User not found or already dropped.");

            userCourse.Status = 6; // Dropped
            userCourse.DateStatusChanged = DateTime.UtcNow;
            userCourse.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // promote from waitlist (and report back)
            var (promoted, waitlistStillExists) = await PromoteFromWaitlistAsync(request.CourseSysId);

            return Ok(new
            {
                message = "User dropped successfully.",
                promoted,               // true if someone moved off the waitlist into the class
                waitlist = waitlistStillExists
            });
        }
        [HttpPost("revert-cancel")]
        public async Task<IActionResult> RevertCancel([FromBody] Course courseInput)
        {
            if (courseInput == null || courseInput.CourseSysId == 0)
                return BadRequest("Invalid course id.");

            var course = await _context.Courses.FindAsync(courseInput.CourseSysId);
            if (course == null) return NotFound("Course not found");

            course.Cancelled = false;
            course.CancellReason = null; // optional – clear reason
            course.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Course cancellation reverted." });
        }


        // Add helper to list registered users for a course
        [HttpGet("registered-users")]
        public async Task<IActionResult> GetRegisteredUsers(
    [FromQuery] int courseId,
    [FromQuery] string? lastName = null,
    [FromQuery] string? email = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 15)
        {
            var query = from uc in _context.UserCourses
                        join u in _context.Users on uc.UserSysId equals u.UserSysId
                        where uc.CourseSysId == courseId && uc.Status == 1
                        select new
                        {
                            uc.UserSysId,
                            uc.Attended,
                            u.FirstName,
                            u.Mi,
                            u.LastName,
                            u.Email,
                            u.Title,
                            u.Organization,
                            Role = u.Role.ToString()
                        };

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(u => EF.Functions.Like(u.LastName, $"%{lastName}%"));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(u => EF.Functions.Like(u.Email, $"%{email}%"));

            var total = await query.CountAsync();

            var pagedUsers = await query
                .OrderBy(u => u.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = pagedUsers.Select(u => new {
                u.UserSysId,
                FullName = (u.FirstName + " " + (u.Mi ?? "") + " " + u.LastName).Trim(),
                u.Email,
                u.Role,
                u.Attended,
                u.Title,
                u.Organization
            });

            return Ok(new { data = result, total });
        }

        [HttpPost("cancel")]
public async Task<IActionResult> CancelCourse([FromBody] Course courseInput)
{
    var course = await _context.Courses.FindAsync(courseInput.CourseSysId);

    if (course == null)
        return NotFound("Course not found");

    // Update cancellation details
    course.Cancelled = true;
    course.CancellReason = courseInput.CancellReason;
    course.DateModified = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    // TODO: Email logic here if needed
    return Ok("Course cancelled successfully");
}

        [HttpPut("updateDelivered/{id}")]
        public async Task<IActionResult> UpdateDelivered(int id, [FromBody] Course courseUpdate)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.Delivered = courseUpdate.Delivered;
            course.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("updateApproval/{id}")]
        public async Task<IActionResult> UpdateApproval(int id, [FromBody] Course courseUpdate)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.Approve = courseUpdate.Approve;
            course.ApproveDt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

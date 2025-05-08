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
    DateTime? toDate = null
)
        {
            try
            {
                var query = from c in _context.Courses
                            where c.Cancelled == false || c.Cancelled == null
                            join s in _context.Sites on c.SiteSysId equals s.SiteSysId into siteJoin
                            from s in siteJoin.DefaultIfEmpty()

                            join subj in _context.Subjects on c.SubjectSysId equals subj.SubjectSysId into subjectJoin
                            join i1 in _context.Users on c.Instructor1 equals i1.UserSysId into i1Join
                            from i1 in i1Join.DefaultIfEmpty()

                            join i2 in _context.Users on c.Instructor2 equals i2.UserSysId into i2Join
                            from i2 in i2Join.DefaultIfEmpty()
                            from subj in subjectJoin.DefaultIfEmpty()

                            let regionName = (from r in _context.LkRegionCnties
                                              where r.Code == c.Region
                                              select r.Value).FirstOrDefault()

                            let categoryName = (from cat in _context.LkCategories
                                                where cat.Code == c.ContractType
                                                select cat.Value).FirstOrDefault()

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
                                SubjectTitle = subj.CourseTitle ?? "N/A",
                                SiteName = s.SiteName ?? "N/A",
                                RegionLabel = regionName ?? "N/A",
                                CategoryLabel = categoryName ?? "N/A",
                                RegisteredCount = _context.UserCourses.Count(uc => uc.CourseSysId == c.CourseSysId && uc.Status == 1),
                                InstructorLabel = (i1.FirstName + " " + (i1.Mi ?? "") + " " + i1.LastName).Trim(),
                                Instructor2Label = (i2.FirstName + " " + (i2.Mi ?? "") + " " + i2.LastName).Trim()
                            };

                if (!string.IsNullOrEmpty(title))
                    query = query.Where(c => c.SubjectTitle.Contains(title));

                if (siteId.HasValue)
                    query = query.Where(c => c.SiteSysId == siteId.Value);

                if (region.HasValue)
                    query = query.Where(c => c.RegionLabel != null && c.RegionLabel != "N/A" && (from r in _context.LkRegionCnties where r.Code == region select r.Value).FirstOrDefault() == c.RegionLabel);

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
                    .OrderByDescending(c => c.CourseDate)
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
        [HttpGet("courseWithSessions/{id}")]
        public async Task<IActionResult> GetCourseWithSessions(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Sessions) // 👈 Navigation now works
                .FirstOrDefaultAsync(c => c.CourseSysId == id);

            if (course == null) return NotFound();

            return Ok(course); // 👈 Directly return full course with sessions
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
            if (existingCourse.MaxSeats.HasValue && existingCourse.MaxSeats > 0)
            {
                var waitlistedUsers = await _context.UserCourses
                    .Where(uc => uc.CourseSysId == id && uc.IsWaitlisted)
                    .OrderBy(uc => uc.WaitlistNumber)
                    .Take(existingCourse.MaxSeats.Value)
                    .ToListAsync();

                foreach (var userCourse in waitlistedUsers)
                {
                    userCourse.IsWaitlisted = false;
                    userCourse.WaitlistNumber = null;
                    existingCourse.MaxSeats--; // Occupy the seat
                }

                await _context.SaveChangesAsync(); // Save waitlist updates
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
            if (request == null) return BadRequest("Invalid data.");

            var existing = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserSysId == request.UserSysId && uc.CourseSysId == request.CourseSysId);

            if (existing != null)
            {
                existing.Status = 1;
                existing.DateModified = DateTime.UtcNow;
                existing.DateStatusChanged = DateTime.UtcNow;
                existing.IsWaitlisted = false;
                await _context.SaveChangesAsync();

                return Ok(new { message = "User re-registered successfully!", token = existing.Token });
            }

            request.DateEntered = DateTime.UtcNow;
            request.DateModified = DateTime.UtcNow;
            request.DateStatusChanged = DateTime.UtcNow;
            request.Status = 1; // Registered
            request.Token = Guid.NewGuid(); // Generate token

            _context.UserCourses.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User added successfully!", token = request.Token });
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

        // ✅ Add this to CourseAdminController.cs

        [HttpPut("drop-user")]
        public async Task<IActionResult> DropUserFromCourse([FromBody] UserCourse request)
        {
            if (request == null || request.UserSysId == 0 || request.CourseSysId == 0)
                return BadRequest("Invalid request.");

            var userCourse = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserSysId == request.UserSysId && uc.CourseSysId == request.CourseSysId && uc.Status == 1);

            if (userCourse == null)
                return NotFound("User not found or already dropped.");

            userCourse.Status = 6; // Dropped
            userCourse.DateStatusChanged = DateTime.UtcNow;
            userCourse.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "User dropped successfully." });
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
                            u.FirstName,
                            u.Mi,
                            u.LastName,
                            u.Email,
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
                u.Role
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

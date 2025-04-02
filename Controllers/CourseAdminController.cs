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
                            join s in _context.Sites on c.SiteSysId equals s.SiteSysId into siteJoin
                            from s in siteJoin.DefaultIfEmpty()

                            join subj in _context.Subjects on c.SubjectSysId equals subj.SubjectSysId into subjectJoin
                            from subj in subjectJoin.DefaultIfEmpty()

                            select new
                            {
                                c.CourseSysId,
                                c.SubjectSysId,
                                c.SiteSysId,
                                c.Region,
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
                                SubjectTitle = subj != null ? subj.CourseTitle : "N/A",
                                SiteName = s != null ? s.SiteName : "N/A"
                            };

                if (!string.IsNullOrEmpty(title))
                    query = query.Where(c => c.SubjectTitle.Contains(title));

                if (siteId.HasValue)
                    query = query.Where(c => c.SiteSysId == siteId.Value);

                if (region.HasValue)
                    query = query.Where(c => c.Region == region);

                if (format.HasValue)
                    query = query.Where(c => c.Format == format);

                if (category.HasValue)
                    query = query.Where(c => c.ContractType == category);

                if (fromDate.HasValue)
                    query = query.Where(c => c.CourseDate >= fromDate);

                if (toDate.HasValue)
                    query = query.Where(c => c.CourseDate <= toDate);

                var totalBeforePaging = await query.CountAsync();
                Console.WriteLine($"Total before pagination: {totalBeforePaging}");

                var pagedCourses = await query
                    .OrderByDescending(c => c.CourseDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                Console.WriteLine($"Returning page {page} with pageSize {pageSize}, total returned: {pagedCourses.Count}");

                return Ok(new { data = pagedCourses, total = totalBeforePaging });
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

            return Ok(new { message = "Course updated successfully!" });
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

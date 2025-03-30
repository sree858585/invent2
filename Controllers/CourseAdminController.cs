// Updated CourseAdminController with region, format, category, and site filtering
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using HIVTraining_Vue.Server.Models;

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
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] Course updatedCourse)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            // Update the fields
            course.SiteSysId = updatedCourse.SiteSysId;
            course.Region = updatedCourse.Region;
            course.SubjectSysId = updatedCourse.SubjectSysId;
            course.Instructor1 = updatedCourse.Instructor1;
            course.Instructor2 = updatedCourse.Instructor2;
            course.CourseDate = updatedCourse.CourseDate;
            course.EndDate = updatedCourse.EndDate;
            course.CourseTimeBegin = updatedCourse.CourseTimeBegin;
            course.CourseTimeEnd = updatedCourse.CourseTimeEnd;
            course.RegDeadLine = updatedCourse.RegDeadLine;
            course.MaxSeats = updatedCourse.MaxSeats;
            course.TrainingLocation = updatedCourse.TrainingLocation;
            course.Deliverable = updatedCourse.Deliverable;
            course.Format = updatedCourse.Format;
            course.Rtc = updatedCourse.Rtc;
            course.Coe = updatedCourse.Coe;
            course.OtherFund = updatedCourse.OtherFund;
            course.Hidden = updatedCourse.Hidden;
            course.Information = updatedCourse.Information;
            course.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
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

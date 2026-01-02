using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HIVTraining_Vue.Server.Requests;

namespace HIVTraining_Vue.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateCourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CreateCourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all lookup data for the course scheduling dropdowns
        /// </summary>
        [HttpGet("lookup")]
        public async Task<IActionResult> GetLookupData()
        {
            var trainingCenters = await _context.Sites
        .Where(s => s.Active) // Fetch only active sites
        .Select(s => new { s.SiteSysId, s.SiteName }) // Get Site ID and Name
        .ToListAsync();

            var regions = await _context.LkRegionCnties
                .Select(r => new { r.Code, r.Value })
                .ToListAsync();

            var categories = await _context.LkCategories
                .Select(c => new { c.Code, c.Value })
                .ToListAsync();

            var subjects = await _context.Subjects
        .Where(s => s.Active)
        .Select(s => new
        {
            s.SubjectSysId,
            s.CourseTitle,
            s.Category  // <-- this is the fix
        })
        .ToListAsync();

            var instructors = await _context.Instructors
                .Where(i => i.Active == true)
                .Select(i => new { i.InstructorSysId, i.Name })
                .ToListAsync();

            var deliverables = await _context.LkDeliverables
                .Select(d => new { d.Id, d.Value })
                .ToListAsync();

            var formats = await _context.LkFormats
                .Select(f => new { f.Code, f.Value })
                .ToListAsync();

            return Ok(new
            {
                TrainingCenters = trainingCenters,  
                Regions = regions,
                Categories = categories,
                Subjects = subjects,
                Instructors = instructors,
                Deliverables = deliverables,
                Formats = formats
            });
        }
        [HttpGet("subjectsByCategory/{categoryCode}")]
        public async Task<IActionResult> GetSubjectsByCategory(int categoryCode)
        {
            var subjects = await _context.Subjects
                .Where(s => s.Active && s.Category == categoryCode)
                .Select(s => new { s.SubjectSysId, s.CourseTitle })
                .ToListAsync();

            return Ok(subjects);
        }

        /// <summary>
        /// Schedule a new course
        /// </summary>
        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleCourse([FromBody] CourseScheduleRequest request)
        {
            if (request?.Course == null)
                return BadRequest("Course data is required.");

            var course = request.Course;
            course.DateEntered = DateTime.UtcNow;
            course.DateModified = DateTime.UtcNow;
            course.MarkAsNewUntil = course.MarkAsNewUntil;

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            if (course.IsMultiSession && request.Sessions != null && request.Sessions.Any())
            {
                var sessions = request.Sessions.Select(s => new CourseSession
                {
                    CourseSysId = course.CourseSysId,
                    SessionDate = s.SessionDate,
                    StartTime = TimeSpan.Parse(s.StartTime),
                    EndTime = TimeSpan.Parse(s.EndTime),
                    SessionUrl = s.SessionUrl,
                    TrainingLocation = s.TrainingLocation
                }).ToList();

                _context.CourseSessions.AddRange(sessions);
                await _context.SaveChangesAsync();
            }
            if (course.CourseTimeBegin.HasValue && course.CourseTimeEnd.HasValue)
            {
                var start = course.CourseTimeBegin.Value;
                var end = course.CourseTimeEnd.Value;

                // total hours in decimal
                double totalHours = (end - start).TotalHours;

                // cannot be negative even if user makes mistake
                if (totalHours < 0) totalHours = 0;

                course.BaseHours = (decimal)Math.Round(totalHours, 2);
            }

            return Ok(new { message = "Course scheduled successfully!", courseId = course.CourseSysId });
        }

    }
}

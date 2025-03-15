using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                .Select(s => new { s.SubjectSysId, s.CourseTitle })
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

        /// <summary>
        /// Schedule a new course
        /// </summary>
        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleCourse([FromBody] Course course)
        {
            if (course == null)
            {
                return BadRequest("Invalid course data.");
            }

            course.DateEntered = DateTime.UtcNow;
            course.DateModified = DateTime.UtcNow;

            // ✅ Ensure all required fields have valid values
            if (course.SiteSysId <= 0 || course.SubjectSysId <= 0 || course.Region <= 0)
            {
                return BadRequest("Missing required fields: SiteSysId, SubjectSysId, Region.");
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Course scheduled successfully!", courseId = course.CourseSysId });
        }

    }
}

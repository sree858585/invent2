using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Models;
using System.Linq;
using System.Threading.Tasks;

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

        // GET: api/course/Format/1
        [HttpGet("Format/{format}")]
        public async Task<IActionResult> GetCoursesByFormat(int format)
        {
            var courses = await _context.Courses
                .Where(c => c.Format == format && !c.Hidden)
                .Include(c => c.Subject) // Include Subject navigation property
                .Select(c => new
                {
                    c.CourseSysId,
                    c.CourseDate,
                    c.CourseTime,
                    c.Information,
                    c.City,
                    c.TrainingLocation,
                    c.MaxSeats,
                    SubjectTitle = c.Subject.CourseTitle,
                    SubjectDescription = c.Subject.Description // Include description
                })
                .ToListAsync();

            return Ok(courses);
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _context.Courses
                .Where(c => !c.Hidden) // Exclude hidden courses
                .Include(c => c.Subject) // Include related Subject data
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

            return Ok(courses);
        }


    }


}

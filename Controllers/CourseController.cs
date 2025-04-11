// using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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

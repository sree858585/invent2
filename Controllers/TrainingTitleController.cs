using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System.Threading.Tasks;
using System.Linq;

namespace HIVTraining_Vue.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingTitleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainingTitleController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTitle([FromBody] Subject model)
        {
            if (string.IsNullOrWhiteSpace(model.CourseTitle))
                return BadRequest("Course title is required.");

            model.Active = true;
            _context.Subjects.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Title created successfully!", subjectId = model.SubjectSysId });
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedTitles([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
                                                        [FromQuery] string? title = null, [FromQuery] int? category = null)
        {
            var query = from s in _context.Subjects
                        join c in _context.LkCategories on s.Category equals c.Code into catJoin
                        from c in catJoin.DefaultIfEmpty()
                        select new
                        {
                            s.SubjectSysId,
                            s.CourseTitle,
                            s.Category,
                            CategoryName = c != null ? c.Value : "Uncategorized"
                        };

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(x => x.CourseTitle.Contains(title));

            if (category.HasValue)
                query = query.Where(x => x.Category == category.Value);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.CourseTitle)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, data });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTitleById(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
                return NotFound();

            return Ok(subject);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateTitle(int id, [FromBody] Subject updated)
        {
            var existing = await _context.Subjects.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.CourseTitle = updated.CourseTitle;
            existing.Description = updated.Description;
            existing.Category = updated.Category;
            existing.Active = updated.Active;
            existing.Ai = updated.Ai;
            existing.Cnecredits = updated.Cnecredits;
            existing.Oasascredits = updated.Oasascredits;
            existing.CreditHrs = updated.CreditHrs;
            existing.Is3rdParty = updated.Is3rdParty;
            existing.A3rdPartyCrseId = updated.A3rdPartyCrseId;
            existing.CertDescription = updated.CertDescription;
            existing.MiscCertDesc = updated.MiscCertDesc;
            existing.VideoUrl = updated.VideoUrl;
            existing.IsOnlineTraining = updated.IsOnlineTraining;

            existing.MarkAsNewUntil = updated.MarkAsNewUntil;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Title updated successfully!" });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteTitle(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound();

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Title deleted successfully!" });
        }
    }
}
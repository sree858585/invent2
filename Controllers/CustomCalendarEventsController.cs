using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;

namespace HIVTraining_Vue.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomCalendarEventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CustomCalendarEventsController(ApplicationDbContext context) => _context = context;

        // =========================
        // Admin CRUD
        // =========================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.CustomCalendarEvents
                .AsNoTracking()
                .OrderByDescending(x => x.StartUtc)
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.CustomCalendarEvents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CustomCalendarEventId == id);

            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomCalendarEvent model)
        {
            if (model == null) return BadRequest();

            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest(new { message = "Title is required." });

            if (model.EndUtc.HasValue && model.EndUtc.Value < model.StartUtc)
                return BadRequest(new { message = "EndUtc cannot be before StartUtc." });

            // Server-owned fields
            model.CustomCalendarEventId = 0;
            model.CreatedUtc = DateTime.UtcNow;
            model.UpdatedUtc = DateTime.UtcNow;

            // Normalize UTC kind (optional but helps)
            model.StartUtc = DateTime.SpecifyKind(model.StartUtc, DateTimeKind.Utc);
            if (model.EndUtc.HasValue)
                model.EndUtc = DateTime.SpecifyKind(model.EndUtc.Value, DateTimeKind.Utc);

            _context.CustomCalendarEvents.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomCalendarEvent model)
        {
            if (model == null) return BadRequest();

            var entity = await _context.CustomCalendarEvents
                .FirstOrDefaultAsync(x => x.CustomCalendarEventId == id);

            if (entity == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest(new { message = "Title is required." });

            if (model.EndUtc.HasValue && model.EndUtc.Value < model.StartUtc)
                return BadRequest(new { message = "EndUtc cannot be before StartUtc." });

            // Update fields (explicit mapping is safer than _context.Update(model))
            entity.Title = model.Title;
            entity.ShortDescription = model.ShortDescription;
            entity.LongDescription = model.LongDescription;
            entity.AllDay = model.AllDay;
            entity.Category = model.Category;
            entity.Url = model.Url;
            entity.Color = model.Color;
            entity.IsActive = model.IsActive;

            entity.StartUtc = DateTime.SpecifyKind(model.StartUtc, DateTimeKind.Utc);
            entity.EndUtc = model.EndUtc.HasValue
                ? DateTime.SpecifyKind(model.EndUtc.Value, DateTimeKind.Utc)
                : null;

            entity.UpdatedUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.CustomCalendarEvents
                .FirstOrDefaultAsync(x => x.CustomCalendarEventId == id);

            if (entity == null) return NotFound();

            _context.CustomCalendarEvents.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // Calendar Feed (FullCalendar format)
        // =========================

        // GET /api/CustomCalendarEvents/calendar?start=...&end=...
        [HttpGet("calendar")]
        public async Task<IActionResult> Calendar([FromQuery] string start, [FromQuery] string end)
        {
            if (!DateTimeOffset.TryParse(start, out var startDto) ||
                !DateTimeOffset.TryParse(end, out var endDto))
            {
                return BadRequest(new { message = "Invalid start/end date format." });
            }

            var from = startDto.UtcDateTime;
            var to = endDto.UtcDateTime;

            var items = await _context.CustomCalendarEvents
                .AsNoTracking()
                .Where(e => e.IsActive
                            && e.StartUtc < to
                            && (e.EndUtc == null || e.EndUtc > from))
                .Select(e => new
                {
                    id = "custom-" + e.CustomCalendarEventId,
                    title = e.Title,
                    start = e.AllDay
                        ? e.StartUtc.ToString("yyyy-MM-dd")
                        : e.StartUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    end = e.EndUtc == null ? null :
                        (e.AllDay
                            ? e.EndUtc.Value.ToString("yyyy-MM-dd")
                            : e.EndUtc.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                    allDay = e.AllDay,
                    backgroundColor = e.Color,
                    borderColor = e.Color,
                    extendedProps = new
                    {
                        source = "custom",
                        customEventId = e.CustomCalendarEventId,
                        shortDescription = e.ShortDescription,
                        longDescription = e.LongDescription,
                        category = e.Category,
                        url = e.Url
                    }
                })
                .ToListAsync();

            return Ok(items);
        }
    }
}
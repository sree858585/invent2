using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using System.Globalization;
using System.Text.Json;

namespace HIVTraining.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingCalendarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TrainingCalendarController(ApplicationDbContext context) => _context = context;

        // GET /api/TrainingCalendar/events?start=2025-12-28T00:00:00-05:00&end=2026-02-08T00:00:00-05:00
        [HttpGet("events")]
        public async Task<IActionResult> GetEvents([FromQuery] string start, [FromQuery] string end)
        {
            // ✅ Robust parsing (handles ISO with timezone offset)
            if (!TryParseIsoDate(start, out var startDto) || !TryParseIsoDate(end, out var endDto))
                return BadRequest(new { message = "Invalid start/end date format." });

            var from = startDto.Date;
            var toExclusive = endDto.Date.AddDays(1);

            // ✅ Courses only (no sessions table)
            var events = await _context.Courses
                .AsNoTracking()
                .Where(c => !c.Hidden
                            && c.CourseDate.HasValue
                            && c.CourseDate.Value.Date >= from
                            && c.CourseDate.Value.Date < toExclusive)
                .Select(c => new
                {
                    id = "course-" + c.CourseSysId,
                    title = c.Subject != null ? c.Subject.CourseTitle : "Training",
                    start = BuildStart(c.CourseDate!.Value, c.CourseTimeBegin),
                    end = BuildEnd(c.CourseDate!.Value, c.CourseTimeEnd),
                    allDay = (c.CourseTimeBegin == null),
                    extendedProps = new
                    {
                        courseSysId = c.CourseSysId,
                        city = c.City,
                        trainingLocation = c.TrainingLocation,
                        virtualUrl = c.VirtualUrl
                    }
                })
                .ToListAsync();

            // ✅ Force plain JSON array output (bypasses ReferenceHandler.Preserve)
            var json = JsonSerializer.Serialize(events, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return Content(json, "application/json");
        }

        private static bool TryParseIsoDate(string s, out DateTimeOffset dto)
        {
            // handles: 2025-12-28T00:00:00-05:00  OR  2025-12-28T00:00:00Z  OR plain date
            return DateTimeOffset.TryParse(
                s,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out dto
            );
        }

        private static string BuildStart(DateTime courseDate, DateTime? timePart)
        {
            // If no time => all-day date string
            if (!timePart.HasValue) return courseDate.Date.ToString("yyyy-MM-dd");

            // Unspecified kind -> no forced timezone conversion
            var dt = new DateTime(
                courseDate.Year, courseDate.Month, courseDate.Day,
                timePart.Value.Hour, timePart.Value.Minute, timePart.Value.Second,
                DateTimeKind.Unspecified
            );

            return dt.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        private static string? BuildEnd(DateTime courseDate, DateTime? timePart)
        {
            if (!timePart.HasValue) return null;

            var dt = new DateTime(
                courseDate.Year, courseDate.Month, courseDate.Day,
                timePart.Value.Hour, timePart.Value.Minute, timePart.Value.Second,
                DateTimeKind.Unspecified
            );

            return dt.ToString("yyyy-MM-ddTHH:mm:ss");
        }
    }
}
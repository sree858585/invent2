using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;

namespace HIVTraining_Vue.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LookupController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("regions")]
        public async Task<IActionResult> GetRegions()
        {
            var regions = await _context.LkRegionCnties.ToListAsync();
            return Ok(regions);
        }
        [HttpGet("topics")]
        public async Task<IActionResult> GetTopics()
        {
            var topics = await _context.LkTopics
                .OrderBy(t => t.SortKey ?? 9999)
                .ThenBy(t => t.Value)
                .Select(t => new { t.Code, t.Value })
                .ToListAsync();

            return Ok(topics);
        }

        [HttpGet("formats")]
        public async Task<IActionResult> GetFormats()
        {
            var formats = await _context.LkFormats.ToListAsync();
            return Ok(formats);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.LkCategories.ToListAsync();
            return Ok(categories);
        }

        [HttpGet("sites")]
        public async Task<IActionResult> GetSites()
        {
            var sites = await _context.Sites.ToListAsync();
            return Ok(sites);
        }

        [HttpGet("ethnicities")]
        public async Task<IActionResult> GetEthnicities()
        {
            var items = await _context.LkEthnicities
                .OrderBy(x => x.Value)
                .Select(x => new { x.Code, x.Value })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("races")]
        public async Task<IActionResult> GetRaces()
        {
            var items = await _context.LkRaces
                .OrderBy(x => x.Value)
                .Select(x => new { x.Code, x.Value })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("educations")]
        public async Task<IActionResult> GetEducations()
        {
            var items = await _context.LkEducations
                .OrderBy(x => x.Value)
                .Select(x => new { x.Code, x.Value })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("genders")]
        public async Task<IActionResult> GetGenders()
        {
            var items = await _context.LkGenders
                .OrderBy(x => x.Value)
                .Select(x => new { x.Code, x.Value })
                .ToListAsync();

            return Ok(items);

            // If you DO NOT have a table, replace above with this:
            // return Ok(new[]
            // {
            //     new { Code = 1, Value = "Male" },
            //     new { Code = 2, Value = "Female" },
            //     new { Code = 3, Value = "Non-binary" },
            //     new { Code = 4, Value = "Prefer not to say" }
            // });
        }
    }
}
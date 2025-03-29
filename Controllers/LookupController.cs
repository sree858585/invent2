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
    }
}
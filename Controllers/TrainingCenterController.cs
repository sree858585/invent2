using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;

namespace HIVTraining_Vue.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TrainingCenterController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TrainingCenterController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var centers = await _context.Sites
            .Select(s => new
            {
                s.SiteSysId,
                s.SiteName,
                s.ShortName,
                s.Address,
                s.City,
                s.State,
                s.Zip,
                s.ContactName,
                s.ContactEmail,
                s.ContactPhone,
                s.Ext,
                s.WebUrl,
                s.Active,
                s.Type,
                s.Description,
                s.ParentSiteId,
                ParentSiteName = _context.Sites
                    .Where(p => p.SiteSysId == s.ParentSiteId)
                    .Select(p => p.SiteName)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(centers);
    }

    [HttpPut("updateActive/{id}")]
    public async Task<IActionResult> UpdateActive(int id, [FromBody] Site updated)
    {
        var center = await _context.Sites.FindAsync(id);
        if (center == null) return NotFound();

        center.Active = updated.Active;
        await _context.SaveChangesAsync();

        return NoContent();
    }
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? name = null,
    [FromQuery] string? zip = null)
    {
        var query = _context.Sites.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => s.SiteName.Contains(name));

        if (!string.IsNullOrWhiteSpace(zip))
            query = query.Where(s => s.Zip.Contains(zip));

        var total = await query.CountAsync();

        var data = await query
            .OrderBy(s => s.SiteName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.SiteSysId,
                s.SiteName,
                s.ShortName,
                s.Description,
                s.Zip,
                s.Active
            })
            .ToListAsync();

        return Ok(new { total, data });
    }
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] Site site)
    {
        _context.Sites.Add(site);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Training Center created successfully." });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var site = await _context.Sites.FindAsync(id);
        if (site == null) return NotFound();
        return Ok(site);
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Site updated)
    {
        var site = await _context.Sites.FindAsync(id);
        if (site == null) return NotFound();

        site.SiteName = updated.SiteName;
        site.ShortName = updated.ShortName;
        site.Description = updated.Description;
        site.Address = updated.Address;
        site.Address2 = updated.Address2;
        site.City = updated.City;
        site.State = updated.State;
        site.Zip = updated.Zip;
        site.ContactName = updated.ContactName;
        site.ContactEmail = updated.ContactEmail;
        site.ContactPhone = updated.ContactPhone;
        site.Ext = updated.Ext;
        site.WebUrl = updated.WebUrl;
        site.Type = updated.Type;
        site.ParentSiteId = updated.ParentSiteId;
        site.Active = updated.Active;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Training Center updated successfully." });
    }

    [HttpGet("contract-types")]
    public async Task<IActionResult> GetContractTypes()
    {
        var types = await _context.LkContractTypes
            .OrderBy(x => x.SortKey)
            .Select(x => new { x.Code, x.Value })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("parent-sites")]
    public async Task<IActionResult> GetParentSites()
    {
        var parents = await _context.Sites
            .Select(s => new { s.SiteSysId, s.SiteName })
            .ToListAsync();

        return Ok(parents);
    }
}
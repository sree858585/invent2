using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Server.Models;
using HIVTraining_Vue.Data;

namespace HIVTraining_Vue.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InstructorManagementController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InstructorManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var instructors = await _context.Instructors
            .Select(i => new {
                i.InstructorSysId,
                i.Name,
                i.Email,
                i.Phone,
                i.CellPhone,
                i.Active,
                i.SiteSysId,
                i.InsNotes, 
                SiteName = i.SiteSysId != null
                    ? _context.Sites.Where(s => s.SiteSysId == i.SiteSysId).Select(s => s.SiteName).FirstOrDefault()
                    : null
            })
            .ToListAsync();

        return Ok(instructors);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? name = null,
    [FromQuery] int? siteSysId = null)
    {
        var query = _context.Instructors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(i => i.Name.Contains(name));

        if (siteSysId.HasValue)
            query = query.Where(i => i.SiteSysId == siteSysId);

        var total = await query.CountAsync();

        var data = await query
            .OrderBy(i => i.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new {
                i.InstructorSysId,
                i.Name,
                i.Email,
                i.Phone,
                i.CellPhone,
                i.Active,
                i.SiteSysId,
                i.InsNotes, 
                SiteName = i.SiteSysId != null
                    ? _context.Sites.Where(s => s.SiteSysId == i.SiteSysId).Select(s => s.SiteName).FirstOrDefault()
                    : null
            })
            .ToListAsync();

        return Ok(new { total, data });
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateInstructor(int id, [FromBody] Instructor updated)
    {
        var instructor = await _context.Instructors.FindAsync(id);
        if (instructor == null) return NotFound();

        instructor.Name = updated.Name;
        instructor.Email = updated.Email;
        instructor.Phone = updated.Phone;
        instructor.CellPhone = updated.CellPhone;
        instructor.SiteSysId = updated.SiteSysId;
        instructor.InsNotes = updated.InsNotes;
        instructor.Active = updated.Active;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Instructor updated successfully!" });
    }

    [HttpPut("updateActive/{id}")]
    public async Task<IActionResult> UpdateActive(int id, [FromBody] Instructor updated)
    {
        var instructor = await _context.Instructors.FindAsync(id);
        if (instructor == null) return NotFound();

        instructor.Active = updated.Active;
        await _context.SaveChangesAsync();

        return NoContent();
    }
    [HttpPut("archive/{id}")]
    public async Task<IActionResult> ArchiveInstructor(int id)
    {
        var instructor = await _context.Instructors.FindAsync(id);
        if (instructor == null) return NotFound();

        instructor.Active = false; 
        await _context.SaveChangesAsync();

        return Ok(new { message = "Instructor archived successfully!" });
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateInstructor([FromBody] Instructor instructor)
    {
        _context.Instructors.Add(instructor);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
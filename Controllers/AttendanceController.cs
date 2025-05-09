using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HIVTraining_Vue.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPut("mark")]
        public async Task<IActionResult> MarkAttendance([FromBody] AttendanceRequest request)
        {
            if (request == null || request.UserSysId == 0 || request.CourseSysId == 0)
                return BadRequest("Invalid data.");

            var record = await _context.UserCourses
                .FirstOrDefaultAsync(uc => uc.UserSysId == request.UserSysId && uc.CourseSysId == request.CourseSysId);

            if (record == null)
                return NotFound("Registration not found.");

            record.Attended = request.Attended;
            record.DateModified = DateTime.UtcNow;
            record.DateStatusChanged = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Attendance updated successfully." });
        }
        [HttpGet("summary")]
        public async Task<IActionResult> GetAttendanceSummary([FromQuery] int courseId)
        {
            var total = await _context.UserCourses
                .Where(uc => uc.CourseSysId == courseId && uc.Status == 1)
                .CountAsync();

            var attended = await _context.UserCourses
                .Where(uc => uc.CourseSysId == courseId && uc.Status == 1 && uc.Attended == true)
                .CountAsync();

            var notAttended = total - attended;

            return Ok(new
            {
                registered = total,
                attended,
                notAttended
            });
        }
    }

    public class AttendanceRequest
    {
        public int UserSysId { get; set; }
        public int CourseSysId { get; set; }
        public bool Attended { get; set; }
    }
}
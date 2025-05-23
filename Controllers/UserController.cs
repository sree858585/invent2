using Microsoft.AspNetCore.Identity;
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
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ GET: Get User by ID with Lookup Values
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            Console.WriteLine($"Fetching user with ID: {id}");

            var user = await _context.Users
                .Where(u => u.UserId == id)
                .Select(u => new
                {
                    u.UserId,
                    u.UserSysId,
                    u.FirstName,
                    u.Mi,
                    u.LastName,
                    u.Email,
                    u.AltEmail,
                    u.Title,
                    u.Organization,
                    u.Country,

                    // 🔥 Fix: Use `Code` for lookup matching, and `Value` for display
                    WorkSetting = _context.LkWorkSettings
                        .Where(ws => ws.Code == u.WorkSetting)
                        .Select(ws => ws.Value)
                        .FirstOrDefault(),

                    Education = _context.LkEducations
                        .Where(e => e.Code == u.Education)
                        .Select(e => e.Value)
                        .FirstOrDefault(),

                    Ethnicity = _context.LkEthnicities
                        .Where(e => e.Code == u.Ethnicity)
                        .Select(e => e.Value)
                        .FirstOrDefault(),

                    Race = _context.LkRaces
                        .Where(r => r.Code == u.Race)
                        .Select(r => r.Value)
                        .FirstOrDefault(),

                    Occupation = _context.LkOccupations
                        .Where(o => o.Code == u.Occupation)
                        .Select(o => o.Value)
                        .FirstOrDefault(),

                    YearsCurrentOccupation = _context.LkYearsCurrentOccupations
                        .Where(y => y.Code == u.YearsCurrentOccupation)
                        .Select(y => y.Value)
                        .FirstOrDefault(),

                    u.Address,
                    u.City,
                    u.State,
                    u.Zip,
                    u.Phone,
                    u.CellPhone,
                    u.WorkPhone,
                    u.WorkPhoneExt,
                    u.Role,
                    u.Active,
                    u.SiteSysId,
                    u.DateEntered,
                    u.DateModified,
                    u.Adaneed,
                    u.Adadetails
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user);
        }

        // ✅ GET: Get All Users (for admin use)
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.UserId,
                    Name = u.FirstName + " " + (u.Mi ?? "") + " " + u.LastName,
                    u.Email,
                    u.Organization,
                    u.Phone
                })
                .ToListAsync();

            return Ok(users);
        }

        // ✅ PUT: Update User (For Future Use)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] User updatedUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.Address = updatedUser.Address;
            user.City = updatedUser.City;
            user.State = updatedUser.State;
            user.Zip = updatedUser.Zip;
            user.Phone = updatedUser.Phone;
            user.WorkPhone = updatedUser.WorkPhone;
            user.Organization = updatedUser.Organization;
            user.Occupation = updatedUser.Occupation;
            user.DateModified = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User updated successfully!",
                userId = user.UserId.ToString()  // ✅ Return updated UserID (GUID)
            });
        }
    }
}

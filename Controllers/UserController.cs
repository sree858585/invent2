// using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

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

        // ========= DTOs =========
        public sealed class UserProfileDto
        {
            public Guid? UserId { get; set; }

            // Names & Email
            public string? FirstName { get; set; }
            public string? Mi { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
            public string? AltEmail { get; set; }

            // NEW fields (codes + labels)
            public int? PronounId { get; set; }
            public string? PronounLabel { get; set; }

            public int? WorkLocationId { get; set; }
            public string? WorkLocationLabel { get; set; }

            public int? WorkSetting { get; set; }
            public string? WorkSettingLabel { get; set; }

            public int? Ethnicity { get; set; }
            public string? EthnicityLabel { get; set; }

            public int? Race { get; set; }
            public string? RaceLabel { get; set; }

            public int? Occupation { get; set; }
            public string? OccupationLabel { get; set; }

            // Phones + flags
            public string? WorkPhone { get; set; }
            public bool? PrimaryCanText { get; set; }
            public string? CellPhone { get; set; }    // Alternate Phone
            public bool? AltCanText { get; set; }
        }

        public sealed class UserUpdateDto
        {
            // Only fields that can be edited from profile modal
            [Required] public string FirstName { get; set; } = default!;
            public string? Mi { get; set; }
            [Required] public string LastName { get; set; } = default!;
            [Required, EmailAddress] public string Email { get; set; } = default!;
            [EmailAddress] public string? AltEmail { get; set; }

            // lookup codes
            public int? PronounId { get; set; }
            public int? WorkLocationId { get; set; }
            public int? WorkSetting { get; set; }
            public int? Ethnicity { get; set; }
            public int? Race { get; set; }
            public int? Occupation { get; set; }

            // phones + flags
            [Required] public string WorkPhone { get; set; } = default!;
            public bool? PrimaryCanText { get; set; }
            public string? CellPhone { get; set; }
            public bool? AltCanText { get; set; }
        }

        // ========= GET: User (codes + labels) =========
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var u = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == id);
            if (u == null) return NotFound(new { message = "User not found" });

            var dto = new UserProfileDto
            {
                UserId = u.UserId,   // Guid? -> Guid? OK

                FirstName = u.FirstName,
                Mi = u.Mi,
                LastName = u.LastName,
                Email = u.Email,
                AltEmail = u.AltEmail,  

                PronounId = u.PronounId,
                // If your LkPronoun uses Name instead of Value, change to .Select(p => p.Name)
                PronounLabel = await _context.LkPronouns
                    .Where(p => p.PronounId == u.PronounId)
                    .Select(p => p.Value)
                    .FirstOrDefaultAsync(),

                WorkLocationId = u.WorkLocationId,
                // FIX: LkWorkLocation has Name (not Value)
                WorkLocationLabel = await _context.LkWorkLocations
                    .Where(w => w.WorkLocationId == u.WorkLocationId)
                    .Select(w => w.Name)
                    .FirstOrDefaultAsync(),

                WorkSetting = u.WorkSetting,
                WorkSettingLabel = await _context.LkWorkSettings
                    .Where(ws => ws.Code == u.WorkSetting)
                    .Select(ws => ws.Value)
                    .FirstOrDefaultAsync(),

                Ethnicity = u.Ethnicity,
                EthnicityLabel = await _context.LkEthnicities
                    .Where(e => e.Code == u.Ethnicity)
                    .Select(e => e.Value)
                    .FirstOrDefaultAsync(),

                Race = u.Race,
                RaceLabel = await _context.LkRaces
                    .Where(r => r.Code == u.Race)
                    .Select(r => r.Value)
                    .FirstOrDefaultAsync(),

                Occupation = u.Occupation,
                OccupationLabel = await _context.LkOccupations
                    .Where(o => o.Code == u.Occupation)
                    .Select(o => o.Value)
                    .FirstOrDefaultAsync(),

                WorkPhone = u.WorkPhone,
                PrimaryCanText = u.PrimaryCanText,

                CellPhone = u.CellPhone,     // Alternate Phone
                AltCanText = u.AltCanText
            };

            return Ok(dto);
        }

        // ========= PUT: Partial Update =========
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto dto)
        {
            var u = await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
            if (u == null) return NotFound(new { message = "User not found" });

            // Basic requireds
            u.FirstName = dto.FirstName;
            u.Mi = string.IsNullOrWhiteSpace(dto.Mi) ? null : dto.Mi;
            u.LastName = dto.LastName;
            u.Email = dto.Email;
            u.AltEmail = string.IsNullOrWhiteSpace(dto.AltEmail)
    ? null
    : dto.AltEmail;
            // lookups (nullable)
            u.PronounId = dto.PronounId;
            u.WorkLocationId = dto.WorkLocationId;
            u.WorkSetting = dto.WorkSetting;
            u.Ethnicity = dto.Ethnicity;
            u.Race = dto.Race;
            u.Occupation = dto.Occupation;

            // phones + flags
            u.WorkPhone = dto.WorkPhone;
            u.PrimaryCanText = dto.PrimaryCanText;
            u.CellPhone = string.IsNullOrWhiteSpace(dto.CellPhone) ? null : dto.CellPhone;
            u.AltCanText = dto.AltCanText;

            u.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // return fresh projection with labels
            return await GetUserById(id);
        }

        // (Optional) simple admin list
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.UserId,
                    Name = u.FirstName + " " + (u.Mi ?? "") + " " + u.LastName,
                    u.Email,
                    u.WorkPhone
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
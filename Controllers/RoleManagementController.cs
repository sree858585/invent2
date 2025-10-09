using HIVTraining_Vue.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HIVTraining_Vue.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleManagementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoleManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/RoleManagement/users
        // GET: api/RoleManagement/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            string? name = null,
            string? email = null,
            int page = 1,
            int pageSize = 10,
            string sortBy = "name",   // "name" | "role"
            string sortDir = "asc")   // "asc" | "desc"
        {
            // Build once so EF translates everything to SQL
            var q = _context.Users.Select(u => new
            {
                u.UserId,
                u.FirstName,
                u.LastName,
                u.Email,

                // Booleans done in SQL (no client eval)
                IsAdmin = _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur, r })
                    .Any(x => x.ur.UserId == u.UserId.ToString() && x.r.Name == "Admin"),

                IsManager = _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur, r })
                    .Any(x => x.ur.UserId == u.UserId.ToString() && x.r.Name == "Manager"),
            })
            .Select(x => new
            {
                x.UserId,
                x.FirstName,
                x.LastName,
                x.Email,
                Role = x.IsAdmin ? "Admin" : x.IsManager ? "Manager" : "User",
                RoleRank = x.IsAdmin ? 3 : x.IsManager ? 2 : 1  // User=1, Manager=2, Admin=3
            });

            // Full-name search (First + Last) and email
            if (!string.IsNullOrWhiteSpace(name))
            {
                var p = $"%{name.Trim()}%";
                q = q.Where(x =>
                    EF.Functions.Like((x.FirstName ?? "") + " " + (x.LastName ?? ""), p) ||
                    EF.Functions.Like((x.LastName ?? "") + " " + (x.FirstName ?? ""), p));
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                var pe = $"%{email.Trim()}%";
                q = q.Where(x => x.Email != null && EF.Functions.Like(x.Email, pe));
            }

            // Sorting
            if (string.Equals(sortBy, "role", StringComparison.OrdinalIgnoreCase))
            {
                q = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase)
                    ? q.OrderByDescending(x => x.RoleRank).ThenBy(x => x.LastName).ThenBy(x => x.FirstName)
                    : q.OrderBy(x => x.RoleRank).ThenBy(x => x.LastName).ThenBy(x => x.FirstName);
            }
            else // name
            {
                q = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase)
                    ? q.OrderByDescending(x => (x.FirstName ?? "") + " " + (x.LastName ?? ""))
                    : q.OrderBy(x => (x.FirstName ?? "") + " " + (x.LastName ?? ""));
            }

            var total = await q.CountAsync();
            var data = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new { total, data });
        }

        // local DTO (can be moved to its own file)
        private sealed class RoleRow
        {
            public Guid? UserId { get; set; }
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Role { get; set; } = "User";
            public int RoleRank { get; set; } // User=0, Manager=1, Admin=2
        }

        public sealed class SetRoleDto
        {
            public string Role { get; set; } = "User"; // "User" | "Manager" | "Admin"
        }

        // PUT: api/RoleManagement/{userId}/role
        [HttpPut("{id:guid}/role")]
        public async Task<IActionResult> SetRole(Guid id, [FromBody] SetRoleDto dto)
        {
            var adminId = await _context.Roles.Where(r => r.Name == "Admin").Select(r => r.Id).FirstOrDefaultAsync();
            var managerId = await _context.Roles.Where(r => r.Name == "Manager").Select(r => r.Id).FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(adminId) || string.IsNullOrEmpty(managerId))
                return BadRequest("Roles 'Admin' and 'Manager' must exist.");

            var userStringId = id.ToString();

            // remove existing Admin/Manager roles (single-role policy)
            var existing = await _context.UserRoles
                .Where(ur => ur.UserId == userStringId && (ur.RoleId == adminId || ur.RoleId == managerId))
                .ToListAsync();

            if (existing.Count > 0)
            {
                _context.UserRoles.RemoveRange(existing);
            }

            // If the chosen role is Admin/Manager, add it; if "User", we simply leave no row.
            if (dto.Role == "Admin" || dto.Role == "Manager")
            {
                var targetRoleId = dto.Role == "Admin" ? adminId : managerId;

                _context.UserRoles.Add(new IdentityUserRole<string>
                {
                    UserId = userStringId,
                    RoleId = targetRoleId!
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Role updated", role = dto.Role });
        }
    }
}
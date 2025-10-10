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
            int pageSize = 10)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 200);

            // 1) Base users (light projection)
            var usersQ = _context.Users.AsNoTracking()
                .Select(u => new
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email
                });

            // 2) Filters (fast LIKE on separate columns)
            if (!string.IsNullOrWhiteSpace(name))
            {
                var term = $"%{name.Trim()}%";
                usersQ = usersQ.Where(x =>
                    (x.FirstName != null && EF.Functions.Like(x.FirstName, term)) ||
                    (x.LastName != null && EF.Functions.Like(x.LastName, term)));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var eTerm = $"%{email.Trim()}%";
                usersQ = usersQ.Where(x => x.Email != null && EF.Functions.Like(x.Email, eTerm));
            }

            // 3) Count before paging
            var total = await usersQ.CountAsync();

            // 4) Fixed, indexed sort + paging (fast & deterministic)
            usersQ = usersQ
                .OrderBy(x => x.LastName ?? "")
                .ThenBy(x => x.FirstName ?? "");

            var pageRows = await usersQ
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5) Resolve roles only for the current page
            var pageIds = pageRows.Select(r => r.UserId!.ToString()).ToList();

            var rolesPageQ =
                from ur in _context.UserRoles.AsNoTracking()
                join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                where pageIds.Contains(ur.UserId)
                select new { ur.UserId, r.Name };

            var roleLookup = await rolesPageQ
                .GroupBy(x => x.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    RoleRank = g.Max(x => x.Name == "Admin" ? 3 :
                                          x.Name == "Manager" ? 2 : 1)
                })
                .ToDictionaryAsync(x => x.UserId, x => x.RoleRank);

            var data = pageRows.Select(x =>
            {
                var rr = roleLookup.TryGetValue(x.UserId!.ToString(), out var rank) ? rank : 1;
                var role = rr == 3 ? "Admin" : rr == 2 ? "Manager" : "User";
                return new
                {
                    x.UserId,
                    x.FirstName,
                    x.LastName,
                    x.Email,
                    Role = role
                };
            }).ToList();

            return Ok(new { total, data });
        }

        // ----- unchanged PUT -----
        public sealed class SetRoleDto { public string Role { get; set; } = "User"; }

        [HttpPut("{id:guid}/role")]
        public async Task<IActionResult> SetRole(Guid id, [FromBody] SetRoleDto dto)
        {
            var adminId = await _context.Roles.Where(r => r.Name == "Admin").Select(r => r.Id).FirstOrDefaultAsync();
            var managerId = await _context.Roles.Where(r => r.Name == "Manager").Select(r => r.Id).FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(adminId) || string.IsNullOrEmpty(managerId))
                return BadRequest("Roles 'Admin' and 'Manager' must exist.");

            var userStringId = id.ToString();

            var existing = await _context.UserRoles
                .Where(ur => ur.UserId == userStringId && (ur.RoleId == adminId || ur.RoleId == managerId))
                .ToListAsync();

            if (existing.Count > 0) _context.UserRoles.RemoveRange(existing);

            if (dto.Role == "Admin" || dto.Role == "Manager")
            {
                var targetRoleId = dto.Role == "Admin" ? adminId : managerId;
                _context.UserRoles.Add(new IdentityUserRole<string> { UserId = userStringId, RoleId = targetRoleId! });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Role updated", role = dto.Role });
        }
    }
}
// using directives
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models; // <-- contains ApplicationUser
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
        private readonly UserManager<ApplicationUser> _userManager; // << correct generic

        public RoleManagementController(ApplicationDbContext context,
                                        UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/RoleManagement/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
    string? name = null,
    string? email = null,
    string role = "All",      // <-- NEW
    int page = 1,
    int pageSize = 10)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Clamp(pageSize, 5, 200);

                // Base Users (light projection)
                var usersQ = _context.Users.AsNoTracking()
                    .Select(u => new
                    {
                        u.UserId,        // Guid?
                        u.FirstName,
                        u.LastName,
                        u.Email
                    });

                // Filters: name / email
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

                // If role filter == "All", use fast path (page before role join)
                if (string.Equals(role, "All", StringComparison.OrdinalIgnoreCase))
                {
                    var total = await usersQ.CountAsync();

                    usersQ = usersQ
                        .OrderBy(x => x.LastName ?? "")
                        .ThenBy(x => x.FirstName ?? "");

                    var pageRows = await usersQ
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    if (pageRows.Count == 0)
                        return Ok(new { total, data = Array.Empty<object>() });

                    var pageIds = pageRows
                        .Where(r => r.UserId.HasValue)
                        .Select(r => r.UserId!.Value.ToString())
                        .ToList();

                    // Roles for the current page
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

                    // Lock info
                    var identityLockInfo = await _userManager.Users.AsNoTracking()
                        .Where(iu => pageIds.Contains(iu.Id))
                        .Select(iu => new { iu.Id, iu.LockoutEnabled, iu.LockoutEnd })
                        .ToListAsync();

                    var lockLookup = identityLockInfo.ToDictionary(
                        k => k.Id,
                        v => v.LockoutEnabled && v.LockoutEnd.HasValue && v.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow
                    );

                    var data = pageRows.Select(x =>
                    {
                        var uid = x.UserId?.ToString();
                        var rr = (uid != null && roleLookup.TryGetValue(uid, out var rank)) ? rank : 1;
                        var roleStr = rr == 3 ? "Admin" : rr == 2 ? "Manager" : "User";
                        var isLocked = (uid != null && lockLookup.TryGetValue(uid, out var locked)) && locked;

                        return new
                        {
                            userId = x.UserId,
                            firstName = x.FirstName ?? "",
                            lastName = x.LastName ?? "",
                            email = x.Email ?? "",
                            role = roleStr,
                            isLocked
                        };
                    }).ToList();

                    return Ok(new { total, data });
                }
                else
                {
                    // Role-specific filter path (compute rank for ALL filtered users, then filter)
                    int wantRank = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? 3
                                 : role.Equals("Manager", StringComparison.OrdinalIgnoreCase) ? 2
                                 : 1; // "User"

                    var rankedQ =
                        from u in usersQ
                        join ur in _context.UserRoles.AsNoTracking()
                            on u.UserId!.ToString() equals ur.UserId into urj
                        from ur in urj.DefaultIfEmpty()
                        join r in _context.Roles.AsNoTracking()
                            on ur.RoleId equals r.Id into rj
                        from r in rj.DefaultIfEmpty()
                        let roleRank = r == null ? 1 : (r.Name == "Admin" ? 3 : r.Name == "Manager" ? 2 : 1)
                        group roleRank by new { u.UserId, u.FirstName, u.LastName, u.Email } into g
                        select new
                        {
                            g.Key.UserId,
                            g.Key.FirstName,
                            g.Key.LastName,
                            g.Key.Email,
                            RoleRank = g.Max()
                        };

                    rankedQ = rankedQ.Where(x => x.RoleRank == wantRank);

                    var total = await rankedQ.CountAsync();

                    rankedQ = rankedQ
                        .OrderBy(x => x.LastName ?? "")
                        .ThenBy(x => x.FirstName ?? "");

                    var pageRows = await rankedQ
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    if (pageRows.Count == 0)
                        return Ok(new { total, data = Array.Empty<object>() });

                    var pageIds = pageRows
                        .Where(rw => rw.UserId.HasValue)
                        .Select(rw => rw.UserId!.Value.ToString())
                        .ToList();

                    // Lock info
                    var identityLockInfo = await _userManager.Users.AsNoTracking()
                        .Where(iu => pageIds.Contains(iu.Id))
                        .Select(iu => new { iu.Id, iu.LockoutEnabled, iu.LockoutEnd })
                        .ToListAsync();

                    var lockLookup = identityLockInfo.ToDictionary(
                        k => k.Id,
                        v => v.LockoutEnabled && v.LockoutEnd.HasValue && v.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow
                    );

                    var data = pageRows.Select(x => new
                    {
                        userId = x.UserId,
                        firstName = x.FirstName ?? "",
                        lastName = x.LastName ?? "",
                        email = x.Email ?? "",
                        role = x.RoleRank == 3 ? "Admin" : x.RoleRank == 2 ? "Manager" : "User",
                        isLocked = lockLookup.TryGetValue(x.UserId!.Value.ToString(), out var locked) && locked
                    }).ToList();

                    return Ok(new { total, data });
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.ToString(), title: "GetUsers failed");
            }
        }

        public sealed class SetRoleDto { public string Role { get; set; } = "User"; }

        // PUT: api/RoleManagement/{id}/role
        [HttpPut("{id:guid}/role")]
        public async Task<IActionResult> SetRole(Guid id, [FromBody] SetRoleDto dto)
        {
            // Make sure roles exist (Admin, Manager)
            var adminId = await _context.Roles.Where(r => r.Name == "Admin").Select(r => r.Id).FirstOrDefaultAsync();
            var managerId = await _context.Roles.Where(r => r.Name == "Manager").Select(r => r.Id).FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(adminId) || string.IsNullOrEmpty(managerId))
                return BadRequest("Roles 'Admin' and 'Manager' must exist.");

            var userStringId = id.ToString();

            // Single-role policy among Admin/Manager: remove any existing admin/manager assignments
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

        public sealed class ToggleLockDto { public bool Lock { get; set; } }

        // PUT: api/RoleManagement/{id}/lock
        [HttpPut("{id:guid}/lock")]
        public async Task<IActionResult> ToggleLock(Guid id, [FromBody] ToggleLockDto body)
        {
            var userStringId = id.ToString();

            var user = await _userManager.FindByIdAsync(userStringId);
            if (user == null) return NotFound("AspNetUser not found");

            if (!user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
                var e = await _userManager.UpdateAsync(user);
                if (!e.Succeeded) return BadRequest("Failed to enable lockout.");
            }

            var end = body.Lock ? DateTimeOffset.UtcNow.AddYears(100) : (DateTimeOffset?)null;
            var r = await _userManager.SetLockoutEndDateAsync(user, end);
            if (!r.Succeeded) return BadRequest("Failed to set lockout end.");

            return Ok(new { message = body.Lock ? "User locked" : "User unlocked", locked = body.Lock });
        }
    }
}
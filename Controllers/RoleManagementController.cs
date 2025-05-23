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

        [HttpGet("admins")]
        public async Task<IActionResult> GetUsersWithAdminStatus(string? lastName = null, string? email = null, int page = 1, int pageSize = 10)
        {
            var adminRoleId = await _context.Roles
                .Where(r => r.Name == "Admin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var managerRoleId = await _context.Roles
                .Where(r => r.Name == "Manager")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(adminRoleId) || string.IsNullOrEmpty(managerRoleId))
                return BadRequest("Required roles not found");

            var baseQuery = _context.Users
                .Select(u => new
                {
                    u.UserSysId,
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    IsAdmin = _context.UserRoles
                        .Any(ur => ur.UserId == u.UserId.ToString() && ur.RoleId == adminRoleId),
                    IsManager = _context.UserRoles
                        .Any(ur => ur.UserId == u.UserId.ToString() && ur.RoleId == managerRoleId)
                });

            if (!string.IsNullOrEmpty(lastName))
                baseQuery = baseQuery.Where(q => q.LastName != null && EF.Functions.Like(q.LastName, $"%{lastName}%"));

            if (!string.IsNullOrEmpty(email))
                baseQuery = baseQuery.Where(q => q.Email != null && EF.Functions.Like(q.Email, $"%{email}%"));

            baseQuery = baseQuery.OrderByDescending(q => q.IsAdmin);

            var total = await baseQuery.CountAsync();
            var data = await baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, data });
        }

        [HttpPost("assign-admin")]
        public async Task<IActionResult> AssignAdmin([FromBody] Guid userId)
        {
            var adminRoleId = await _context.Roles
                .Where(r => r.Name == "Admin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(adminRoleId))
                return BadRequest("Admin role not found");

            var stringUserId = userId.ToString();

            bool alreadyAssigned = await _context.UserRoles
                .AnyAsync(x => x.UserId == stringUserId && x.RoleId == adminRoleId);

            if (alreadyAssigned)
                return Ok("Already an admin");

            _context.UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = stringUserId,
                RoleId = adminRoleId
            });

            await _context.SaveChangesAsync();
            return Ok("Admin role assigned");
        }

        [HttpPost("remove-admin")]
        public async Task<IActionResult> RemoveAdmin([FromBody] Guid userId)
        {
            var adminRoleId = await _context.Roles
                .Where(r => r.Name == "Admin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(adminRoleId))
                return BadRequest("Admin role not found");

            var stringUserId = userId.ToString();

            var entry = await _context.UserRoles
                .FirstOrDefaultAsync(x => x.UserId == stringUserId && x.RoleId == adminRoleId);

            if (entry == null)
                return Ok("User is not an admin");

            _context.UserRoles.Remove(entry);
            await _context.SaveChangesAsync();

            return Ok("Admin role removed");
        }

        [HttpGet("managers")]
        public async Task<IActionResult> GetUsersWithManagerStatus(string? lastName = null, string? email = null, int page = 1, int pageSize = 10)
        {
            var managerRoleId = await _context.Roles
                .Where(r => r.Name == "Manager")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var adminRoleId = await _context.Roles
                .Where(r => r.Name == "Admin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(managerRoleId) || string.IsNullOrEmpty(adminRoleId))
                return BadRequest("Required roles not found");

            var baseQuery = _context.Users
                .Select(u => new
                {
                    u.UserSysId,
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    IsManager = _context.UserRoles
                        .Any(ur => ur.UserId == u.UserId.ToString() && ur.RoleId == managerRoleId),
                    IsAdmin = _context.UserRoles
                        .Any(ur => ur.UserId == u.UserId.ToString() && ur.RoleId == adminRoleId)
                });

            if (!string.IsNullOrEmpty(lastName))
                baseQuery = baseQuery.Where(q => q.LastName != null && EF.Functions.Like(q.LastName, $"%{lastName}%"));

            if (!string.IsNullOrEmpty(email))
                baseQuery = baseQuery.Where(q => q.Email != null && EF.Functions.Like(q.Email, $"%{email}%"));

            baseQuery = baseQuery.OrderByDescending(q => q.IsManager);

            var total = await baseQuery.CountAsync();
            var data = await baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, data });
        }

        [HttpPost("assign-manager")]
        public async Task<IActionResult> AssignManager([FromBody] Guid userId)
        {
            var managerRoleId = await _context.Roles
                .Where(r => r.Name == "Manager")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(managerRoleId))
                return BadRequest("Manager role not found");

            var stringUserId = userId.ToString();

            bool alreadyAssigned = await _context.UserRoles
                .AnyAsync(x => x.UserId == stringUserId && x.RoleId == managerRoleId);

            if (alreadyAssigned)
                return Ok("Already a manager");

            _context.UserRoles.Add(new IdentityUserRole<string>
            {
                UserId = stringUserId,
                RoleId = managerRoleId
            });

            await _context.SaveChangesAsync();
            return Ok("Manager role assigned");
        }

        [HttpPost("remove-manager")]
        public async Task<IActionResult> RemoveManager([FromBody] Guid userId)
        {
            var managerRoleId = await _context.Roles
                .Where(r => r.Name == "Manager")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(managerRoleId))
                return BadRequest("Manager role not found");

            var stringUserId = userId.ToString();

            var entry = await _context.UserRoles
                .FirstOrDefaultAsync(x => x.UserId == stringUserId && x.RoleId == managerRoleId);

            if (entry == null)
                return Ok("User is not a manager");

            _context.UserRoles.Remove(entry);
            await _context.SaveChangesAsync();

            return Ok("Manager role removed");
        }
    }
}
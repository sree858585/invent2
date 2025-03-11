using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HIVTraining_Vue.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RegistrationController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ GET Lookups for Dropdowns
        [HttpGet("lookups")]
        public async Task<IActionResult> GetLookups()
        {
            var lookups = new
            {
                WorkSettings = await _context.LkWorkSettings.Where(ws => ws.IsActive).ToListAsync(),
                EducationLevels = await _context.LkEducations.ToListAsync(),
                Ethnicities = await _context.LkEthnicities.ToListAsync(),
                Races = await _context.LkRaces.ToListAsync(),
                Occupations = await _context.LkOccupations.Where(o => o.IsActive).ToListAsync(),
                YearsCurrentOccupation = await _context.LkYearsCurrentOccupations.ToListAsync()
            };

            return Ok(lookups);
        }

        // ✅ POST Register a New User
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] JsonElement userData)
        {
            try
            {
                // ✅ Extract Values Safely
                string email = GetJsonProperty(userData, "email");
                string password = GetJsonProperty(userData, "password"); // 🔥 Now Password is sent from frontend
                string passwordRecoveryQuestion = GetJsonProperty(userData, "passwordRecoveryQuestion");
                string passwordRecoveryAnswer = GetJsonProperty(userData, "passwordRecoveryAnswer");

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(passwordRecoveryQuestion) || string.IsNullOrWhiteSpace(passwordRecoveryAnswer))
                {
                    return BadRequest(new { message = "Email, password, password recovery question, and answer are required." });
                }

                // ✅ Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User already exists with this email." });
                }

                // ✅ Create Identity User
                var identityUser = new IdentityUser
                {
                    UserName = email,
                    NormalizedUserName = email.ToUpper(),
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    EmailConfirmed = true  // ✅ Auto-confirm email to allow login
                };

                var result = await _userManager.CreateAsync(identityUser, password);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Failed to create user in Identity", errors = result.Errors });
                }

                // ✅ Store Security Question & Answer in Identity Claims
                await _userManager.AddClaimAsync(identityUser, new Claim("PasswordRecoveryQuestion", passwordRecoveryQuestion));
                await _userManager.AddClaimAsync(identityUser, new Claim("PasswordRecoveryAnswer", EncryptData(passwordRecoveryAnswer)));

                try
                {
                    // ✅ Insert into dbo.Users (Handled Missing Properties)
                    var newUser = new User
                    {
                        UserId = Guid.Parse(identityUser.Id),
                        FirstName = GetJsonProperty(userData, "firstName"),
                        LastName = GetJsonProperty(userData, "lastName"),
                        Email = email,
                        Address = GetJsonProperty(userData, "address"),
                        City = GetJsonProperty(userData, "city"),
                        State = GetJsonProperty(userData, "state"),
                        Zip = GetJsonProperty(userData, "zip"),
                        Country = GetJsonProperty(userData, "country"),
                        Phone = GetJsonProperty(userData, "phone"),
                        WorkPhone = GetJsonProperty(userData, "workPhone"),
                        Organization = GetJsonProperty(userData, "organization"),
                        WorkSetting = GetJsonIntProperty(userData, "workSetting"),
                        Education = GetJsonIntProperty(userData, "education"),
                        Ethnicity = GetJsonIntProperty(userData, "ethnicity"),
                        Race = GetJsonIntProperty(userData, "race"),
                        Occupation = GetJsonIntProperty(userData, "occupation"),
                        YearsCurrentOccupation = GetJsonIntProperty(userData, "yearsCurrentOccupation"),
                        DateEntered = DateTime.UtcNow,
                        DateModified = DateTime.UtcNow,
                        Active = true,
                        Role = Guid.NewGuid()
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // ❗ If inserting into dbo.Users fails, delete from Identity
                    await _userManager.DeleteAsync(identityUser);
                    return StatusCode(500, $"Failed to save user details in dbo.Users. Rolling back. Error: {ex.Message}");
                }

                return Ok(new { message = "User registered successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        // ✅ Get JSON String Property (Handles Missing Keys)
        private string GetJsonProperty(JsonElement json, string propertyName)
        {
            return json.TryGetProperty(propertyName, out JsonElement value) ? value.GetString() ?? string.Empty : string.Empty;
        }

        // ✅ Get JSON Integer Property (Handles Missing Keys)
        private int? GetJsonIntProperty(JsonElement json, string propertyName)
        {
            return json.TryGetProperty(propertyName, out JsonElement value) && value.TryGetInt32(out int intValue) ? intValue : (int?)null;
        }

        // ✅ Encryption Method for Password Answer
        private string EncryptData(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}

using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using HIVTraining_Vue.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HIVTraining_Vue.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public LoginController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context,
    IEmailService emailService,
    IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var userDetails = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (userDetails == null)
            {
                return Unauthorized(new { message = "User details not found" });
            }

            // update AspNetUsers.LastLoginDate
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                userId = userDetails.UserId.ToString(),
                firstName = userDetails.FirstName,
                lastName = userDetails.LastName,
                email = user.Email,
                token = token,
                role = role
            });
        }

        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            await _emailService.SendEmailAsync(
                "hemanthgara.hg@gmail.com",
                "SMTP Test",
                "<h2>SMTP Working</h2>"
            );

            return Ok("Sent");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest(new { message = "Email is required." });

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Ok(new { message = "If the email exists, a reset link has been sent." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(model.Email);
            var frontendBaseUrl = _configuration["FrontendBaseUrl"];

            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                frontendBaseUrl = "http://localhost:5173";
            }

            var resetLink =
    $"{frontendBaseUrl}/reset-password?email={encodedEmail}&token={encodedToken}";

            var subject = "Password Reset Request - HIV Training Portal";

            var body = $@"
<!DOCTYPE html>
<html>
<body style='margin:0; padding:0; background:#f4f4f7; font-family:Segoe UI, Arial, sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f4f7; padding:30px 0;'>
<tr>
<td align='center'>

<table width='620' cellpadding='0' cellspacing='0' style='background:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 8px 24px rgba(0,0,0,0.08);'>

<tr>
<td style='background:#43285D; padding:24px 30px; color:#ffffff;'>
<h2 style='margin:0; font-size:24px;'>HIV Training Portal</h2>
<p style='margin:6px 0 0; font-size:14px;'>Password Reset Notification</p>
</td>
</tr>

<tr>
<td style='padding:30px; color:#333333;'>
<h3 style='margin-top:0; color:#43285D;'>Reset Your Password</h3>

<p>Hello,</p>

<p>We received a request to reset the password for your HIV Training Portal account.</p>

<p>Please click the button below to create a new password:</p>

<p style='text-align:center; margin:30px 0;'>
<a href='{resetLink}'
   style='background:#43285D; color:#ffffff; padding:14px 28px; text-decoration:none; border-radius:8px; font-weight:600; display:inline-block;'>
Reset Password
</a>
</p>

<p>If the button does not work, copy and paste this link into your browser:</p>

<p style='word-break:break-all; font-size:13px; color:#555555;'>
{resetLink}
</p>

<p style='margin-top:24px;'>If you did not request this password reset, you can safely ignore this email.</p>

<p style='margin-top:28px;'>
Thank you,<br/>
<strong>HIV Training Support Team</strong><br/>
New York State Department of Health<br/>
Email: support@example.com<br/>
Phone: 000-000-0000
</p>
</td>
</tr>

<tr>
<td style='background:#f4eff9; padding:16px 30px; font-size:12px; color:#6b7280; text-align:center;'>
This is an automated message. Please do not reply to this email.
</td>
</tr>

</table>

</td>
</tr>
</table>

</body>
</html>";
            try
            {
                await _emailService.SendEmailAsync(user.Email!, subject, body);

                return Ok(new
                {
                    message = "Password reset link has been sent to your email."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Reset link was generated, but email failed to send.",
                    error = ex.Message,
                    resetLink
                });
            }
        }

        public class ForgotPasswordRequest
        {
            public string Email { get; set; } = "";
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            if (model == null ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Token) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return BadRequest(new { message = "Email, token, and new password are required." });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return BadRequest(new { message = "Invalid reset request." });
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.NewPassword
            );

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = string.Join(", ", result.Errors.Select(e => e.Description))
                });
            }

            var subject = "Password Changed Successfully - HIV Training Portal";

            var body = $@"
<!DOCTYPE html>
<html>
<body style='margin:0; padding:0; background:#f4f4f7; font-family:Segoe UI, Arial, sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f4f7; padding:30px 0;'>
<tr>
<td align='center'>

<table width='620' cellpadding='0' cellspacing='0' style='background:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 8px 24px rgba(0,0,0,0.08);'>

<tr>
<td style='background:#43285D; padding:24px 30px; color:#ffffff;'>
<h2 style='margin:0; font-size:24px;'>HIV Training Portal</h2>
<p style='margin:6px 0 0; font-size:14px;'>Password Change Confirmation</p>
</td>
</tr>

<tr>
<td style='padding:30px; color:#333333;'>
<h3 style='margin-top:0; color:#43285D;'>Your Password Was Changed Successfully</h3>

<p>Hello,</p>

<p>This is a confirmation that the password for your HIV Training Portal account was successfully changed.</p>

<p><strong>Account:</strong> {model.Email}</p>
<p><strong>Date/Time:</strong> {DateTime.Now:MM/dd/yyyy hh:mm tt}</p>

<p>If you made this change, no further action is needed.</p>

<p style='background:#fff7ed; border-left:4px solid #f97316; padding:12px 14px; margin-top:20px;'>
If you did not change your password, please contact the HIV Training Support Team immediately.
</p>

<p style='margin-top:28px;'>
Thank you,<br/>
<strong>HIV Training Support Team</strong><br/>
New York State Department of Health<br/>
Email: support@example.com<br/>
Phone: 000-000-0000
</p>
</td>
</tr>

<tr>
<td style='background:#f4eff9; padding:16px 30px; font-size:12px; color:#6b7280; text-align:center;'>
This is an automated message. Please do not reply to this email.
</td>
</tr>

</table>

</td>
</tr>
</table>

</body>
</html>";

            try
            {
                await _emailService.SendEmailAsync(user.Email!, subject, body);
            }
            catch
            {
                // Do not fail password reset if confirmation email fails.
            }

            return Ok(new
            {
                message = "Password reset successfully. Redirecting to login..."
            });
        }



        public class ResetPasswordRequest
        {
            public string Email { get; set; } = "";
            public string Token { get; set; } = "";
            public string NewPassword { get; set; } = "";
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Your_Secret_Key_Here"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: "yourdomain.com",
                audience: "yourdomain.com",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class LoginRequest
        {
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
        }
    }
}
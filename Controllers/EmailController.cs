using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace HIVTraining_Vue.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public EmailController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmailToRegisteredUsers([FromBody] SendCourseEmailRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request." });

            if (request.UserIds == null || !request.UserIds.Any())
                return BadRequest(new { message = "Please select at least one user." });

            if (request.CourseId <= 0)
                return BadRequest(new { message = "Invalid course." });

            if (string.IsNullOrWhiteSpace(request.Subject))
                return BadRequest(new { message = "Subject is required." });

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Message is required." });

            var course = await _context.Courses
                .Include(c => c.Subject)
                .Where(c => c.CourseSysId == request.CourseId)
                .Select(c => new
                {
                    CourseTitle = c.Subject != null ? c.Subject.CourseTitle : "Training Course",
                    c.CourseDate,
                    c.EndDate,
                    c.CourseTime,
                    c.CourseTimeBegin,
                    c.CourseTimeEnd,
                    c.TrainingLocation,
                    c.City,
                    c.VirtualUrl,
                    c.Format,

                    FormatLabel = _context.LkFormats
                        .Where(f => f.Code == c.Format)
                        .Select(f => f.Value)
                        .FirstOrDefault(),

                    SiteName = _context.Sites
                        .Where(s => s.SiteSysId == c.SiteSysId)
                        .Select(s => s.SiteName)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (course == null)
                return NotFound(new { message = "Course not found." });

            var users = await _context.Users
                .Where(u => request.UserIds.Contains(u.UserSysId))
                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                .Select(u => new
                {
                    u.UserSysId,
                    u.FirstName,
                    u.LastName,
                    u.Email
                })
                .ToListAsync();

            if (!users.Any())
                return BadRequest(new { message = "No valid user emails found." });

            var sentCount = 0;
            var failedEmails = new List<string>();

            foreach (var user in users)
            {
                var fullName = $"{user.FirstName} {user.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                    fullName = "Participant";

                var htmlBody = BuildCourseEmailTemplate(
                    fullName,
                    course.CourseTitle,
                    course.CourseDate,
                    course.EndDate,
                    course.CourseTime,
                    course.CourseTimeBegin,
                    course.CourseTimeEnd,
                    course.FormatLabel,
                    course.SiteName,
                    course.TrainingLocation,
                    course.City,
                    course.VirtualUrl,
                    request.Message
                );

                try
                {
                    await _emailService.SendEmailAsync(
                        user.Email!,
                        request.Subject,
                        htmlBody,
                        request.Cc
                    );

                    sentCount++;
                }
                catch
                {
                    failedEmails.Add(user.Email!);
                }
            }

            return Ok(new
            {
                message = $"Email sent to {sentCount} user(s).",
                sentCount,
                failedCount = failedEmails.Count,
                failedEmails
            });
        }

        [HttpPost("cancel-course")]
        public async Task<IActionResult> CancelCourseAndEmailUsers(
    [FromBody] CancelCourseEmailRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request." });

            if (request.CourseId <= 0)
                return BadRequest(new { message = "Invalid course." });

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Cancellation message is required." });

            var course = await _context.Courses
                .Include(c => c.Subject)
                .FirstOrDefaultAsync(c => c.CourseSysId == request.CourseId);

            if (course == null)
                return NotFound(new { message = "Course not found." });

            var formatLabel = await _context.LkFormats
                .Where(f => f.Code == course.Format)
                .Select(f => f.Value)
                .FirstOrDefaultAsync();

            var siteName = await _context.Sites
                .Where(s => s.SiteSysId == course.SiteSysId)
                .Select(s => s.SiteName)
                .FirstOrDefaultAsync();

            var users = await (
                from uc in _context.UserCourses
                join u in _context.Users
                    on uc.UserSysId equals u.UserSysId
                where uc.CourseSysId == request.CourseId
                      && uc.Status == 1
                      && !uc.IsWaitlisted
                      && !string.IsNullOrWhiteSpace(u.Email)
                select new
                {
                    u.UserSysId,
                    u.FirstName,
                    u.LastName,
                    u.Email
                }
            )
            .Distinct()
            .ToListAsync();

            course.Cancelled = true;
            course.CancellReason = request.Message;
            course.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var courseTitle = course.Subject?.CourseTitle ?? "Training Course";

            var subject = $"Course Cancelled - {courseTitle}";

            var sentCount = 0;
            var failedEmails = new List<string>();

            foreach (var user in users)
            {
                var fullName = $"{user.FirstName} {user.LastName}".Trim();

                if (string.IsNullOrWhiteSpace(fullName))
                    fullName = "Participant";

                var htmlBody = BuildCancellationEmailTemplate(
                    fullName,
                    courseTitle,
                    course.CourseDate,
                    course.EndDate,
                    course.CourseTime,
                    course.CourseTimeBegin,
                    course.CourseTimeEnd,
                    formatLabel,
                    siteName,
                    course.TrainingLocation,
                    course.City,
                    course.VirtualUrl,
                    request.Message
                );

                try
                {
                    await _emailService.SendEmailAsync(
                        user.Email!,
                        subject,
                        htmlBody
                    );

                    sentCount++;
                }
                catch
                {
                    failedEmails.Add(user.Email!);
                }
            }

            return Ok(new
            {
                message = "Course cancelled successfully.",
                courseCancelled = true,
                registeredUsers = users.Count,
                sentCount,
                failedCount = failedEmails.Count,
                failedEmails
            });
        }

        private static string BuildCourseEmailTemplate(
            string fullName,
            string courseTitle,
            DateTime? courseDate,
            DateTime? endDate,
            string? courseTime,
            DateTime? courseTimeBegin,
            DateTime? courseTimeEnd,
            string? formatLabel,
            string? siteName,
            string? trainingLocation,
            string? city,
            string? virtualUrl,
            string message)
        {
            var dateText = courseDate?.ToString("MM/dd/yyyy") ?? "N/A";

            if (courseDate.HasValue && endDate.HasValue && courseDate.Value.Date != endDate.Value.Date)
            {
                dateText = $"{courseDate.Value:MM/dd/yyyy} - {endDate.Value:MM/dd/yyyy}";
            }

            var timeText = !string.IsNullOrWhiteSpace(courseTime)
                ? courseTime
                : $"{FormatTime(courseTimeBegin)} - {FormatTime(courseTimeEnd)}";

            var isOnline =
                string.Equals(formatLabel, "Online", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(formatLabel, "Webinar", StringComparison.OrdinalIgnoreCase);

            var locationHtml = isOnline
                ? $@"
<tr>
<td style='padding:8px 0; font-weight:600;'>Training Link:</td>
<td style='padding:8px 0;'>
{(!string.IsNullOrWhiteSpace(virtualUrl)
    ? $"<a href='{WebUtility.HtmlEncode(virtualUrl)}' style='color:#43285D;'>Open Training Link</a>"
    : "N/A")}
</td>
</tr>"
                : $@"
<tr>
<td style='padding:8px 0; font-weight:600;'>Site:</td>
<td style='padding:8px 0;'>{WebUtility.HtmlEncode(siteName ?? "N/A")}</td>
</tr>
<tr>
<td style='padding:8px 0; font-weight:600;'>Location:</td>
<td style='padding:8px 0;'>{WebUtility.HtmlEncode(trainingLocation ?? city ?? "N/A")}</td>
</tr>";

            return $@"
<!DOCTYPE html>
<html>
<body style='margin:0; padding:0; background:#f4f4f7; font-family:Segoe UI, Arial, sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0' style='background:#f4f4f7; padding:30px 0;'>
<tr>
<td align='center'>

<table width='640' cellpadding='0' cellspacing='0' style='background:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 8px 24px rgba(0,0,0,0.08);'>

<tr>
<td style='background:#43285D; padding:24px 30px; color:#ffffff;'>
<h2 style='margin:0; font-size:24px;'>HIV Training Portal</h2>
<p style='margin:6px 0 0; font-size:14px;'>Course Communication</p>
</td>
</tr>

<tr>
<td style='padding:30px; color:#333333;'>

<h3 style='margin-top:0; color:#43285D;'>Hello {WebUtility.HtmlEncode(fullName)},</h3>

<div style='font-size:15px; line-height:1.6;'>
{message}
</div>

<h3 style='color:#43285D; margin-top:28px;'>Course Details</h3>

<table width='100%' cellpadding='0' cellspacing='0'>
<tr>
<td style='padding:8px 0; font-weight:600; width:150px;'>Course:</td>
<td style='padding:8px 0;'>{WebUtility.HtmlEncode(courseTitle)}</td>
</tr>
<tr>
<td style='padding:8px 0; font-weight:600;'>Date:</td>
<td style='padding:8px 0;'>{dateText}</td>
</tr>
<tr>
<td style='padding:8px 0; font-weight:600;'>Time:</td>
<td style='padding:8px 0;'>{WebUtility.HtmlEncode(timeText)}</td>
</tr>
<tr>
<td style='padding:8px 0; font-weight:600;'>Format:</td>
<td style='padding:8px 0;'>{WebUtility.HtmlEncode(formatLabel ?? "N/A")}</td>
</tr>
{locationHtml}
</table>

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
        }

        private static string BuildCancellationEmailTemplate(
    string fullName,
    string courseTitle,
    DateTime? courseDate,
    DateTime? endDate,
    string? courseTime,
    DateTime? courseTimeBegin,
    DateTime? courseTimeEnd,
    string? formatLabel,
    string? siteName,
    string? trainingLocation,
    string? city,
    string? virtualUrl,
    string message)
        {
            var dateText = courseDate?.ToString("MM/dd/yyyy") ?? "N/A";

            if (courseDate.HasValue &&
                endDate.HasValue &&
                courseDate.Value.Date != endDate.Value.Date)
            {
                dateText =
                    $"{courseDate.Value:MM/dd/yyyy} - {endDate.Value:MM/dd/yyyy}";
            }

            var timeText = !string.IsNullOrWhiteSpace(courseTime)
                ? courseTime
                : $"{FormatTime(courseTimeBegin)} - {FormatTime(courseTimeEnd)}";

            var isOnline =
                string.Equals(
                    formatLabel,
                    "Online",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    formatLabel,
                    "Webinar",
                    StringComparison.OrdinalIgnoreCase);

            var locationHtml = isOnline
                ? $@"
<tr>
<td style='padding:8px 0; font-weight:600;'>Training Type:</td>
<td style='padding:8px 0;'>
{WebUtility.HtmlEncode(formatLabel ?? "Online")}
</td>
</tr>"
                : $@"
<tr>
<td style='padding:8px 0; font-weight:600;'>Site:</td>
<td style='padding:8px 0;'>
{WebUtility.HtmlEncode(siteName ?? "N/A")}
</td>
</tr>

<tr>
<td style='padding:8px 0; font-weight:600;'>Location:</td>
<td style='padding:8px 0;'>
{WebUtility.HtmlEncode(
            trainingLocation ?? city ?? "N/A")}
</td>
</tr>";

            return $@"
<!DOCTYPE html>
<html>

<body style='
    margin:0;
    padding:0;
    background:#f4f4f7;
    font-family:Segoe UI, Arial, sans-serif;'>

<table
    width='100%'
    cellpadding='0'
    cellspacing='0'
    style='background:#f4f4f7; padding:30px 0;'>

<tr>
<td align='center'>

<table
    width='640'
    cellpadding='0'
    cellspacing='0'
    style='
        background:#ffffff;
        border-radius:14px;
        overflow:hidden;
        box-shadow:0 8px 24px rgba(0,0,0,0.08);'>

<tr>
<td style='
    background:#43285D;
    padding:24px 30px;
    color:#ffffff;'>

<h2 style='margin:0; font-size:24px;'>
HIV Training Portal
</h2>

<p style='margin:6px 0 0; font-size:14px;'>
Course Cancellation Notification
</p>

</td>
</tr>

<tr>
<td style='padding:30px; color:#333333;'>

<h3 style='margin-top:0; color:#43285D;'>
Hello {WebUtility.HtmlEncode(fullName)},
</h3>

<div style='
    padding:14px;
    background:#fef2f2;
    color:#991b1b;
    border-radius:10px;
    font-weight:700;
    margin-bottom:20px;'>

This course has been cancelled.

</div>

<div style='font-size:15px; line-height:1.6;'>
{message}
</div>

<h3 style='
    color:#43285D;
    margin-top:28px;'>

Course Details

</h3>

<table width='100%' cellpadding='0' cellspacing='0'>

<tr>
<td style='
    padding:8px 0;
    font-weight:600;
    width:150px;'>

Course:

</td>

<td style='padding:8px 0;'>
{WebUtility.HtmlEncode(courseTitle)}
</td>
</tr>

<tr>
<td style='padding:8px 0; font-weight:600;'>
Date:
</td>

<td style='padding:8px 0;'>
{dateText}
</td>
</tr>

<tr>
<td style='padding:8px 0; font-weight:600;'>
Time:
</td>

<td style='padding:8px 0;'>
{WebUtility.HtmlEncode(timeText)}
</td>
</tr>

<tr>
<td style='padding:8px 0; font-weight:600;'>
Format:
</td>

<td style='padding:8px 0;'>
{WebUtility.HtmlEncode(formatLabel ?? "N/A")}
</td>
</tr>

{locationHtml}

</table>

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

<td style='
    background:#f4eff9;
    padding:16px 30px;
    font-size:12px;
    color:#6b7280;
    text-align:center;'>

This is an automated message. Please do not reply to this email.

</td>

</tr>

</table>

</td>
</tr>

</table>

</body>
</html>";
        }

        private static string FormatTime(DateTime? time)
        {
            if (!time.HasValue) return "N/A";
            return time.Value.ToString("hh:mm tt");
        }
    }

    public class SendCourseEmailRequest
    {
        public List<int> UserIds { get; set; } = new();
        public int CourseId { get; set; }
        public string Subject { get; set; } = "";
        public string Message { get; set; } = "";
        public string? Cc { get; set; }
    }

    public class CancelCourseEmailRequest
    {
        public int CourseId { get; set; }

        public string Message { get; set; } = "";
    }
}
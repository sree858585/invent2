using System.Text;
using HIVTraining_Vue.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HIVTraining.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private class AttendanceParticipationRow
        {
            public string? CourseTitle { get; set; }
            public DateTime? CourseDate { get; set; }
            public string? SiteName { get; set; }
            public string? FormatLabel { get; set; }
            public int? MaxSeats { get; set; }
            public int RegisteredCount { get; set; }
            public int AttendedCount { get; set; }
        }

        private class DeliveredScheduledRatioRow
        {
            public string? CourseTitle { get; set; }
            public DateTime? CourseDate { get; set; }
            public string? SiteName { get; set; }
            public string? FormatLabel { get; set; }
            public string? Location { get; set; }
            public bool? Delivered { get; set; }
        }

        private class DayOfWeekDeliveredRow
        {
            public string? DayName { get; set; }
            public int DayNumber { get; set; }
            public int DeliveredCount { get; set; }
        }

        private class TrainingDeliveryFrequencyRow
        {
            public string? CourseTitle { get; set; }
            public int DeliveredCount { get; set; }
        }
        private class PopularTrainingRow

        {

            public string? CourseTitle { get; set; }

            public int TotalAttendance { get; set; }

        }

        private class AverageAttendanceRow
        {
            public string? CourseTitle { get; set; }
            public int DeliveredSessions { get; set; }
            public int TotalAttendance { get; set; }
            public double AverageAttendance { get; set; }
        }
        private class CancelledByDayRow
        {
            public string? DayName { get; set; }
            public int DayNumber { get; set; }
            public int CancelledCount { get; set; }
        }

        private class CancelledTrainingRow
        {
            public string? CourseTitle { get; set; }
            public DateTime? CourseDate { get; set; }
            public string? SiteName { get; set; }
            public string? FormatLabel { get; set; }
            public string? Location { get; set; }
        }

        private class TrainingByMonthRow
        {
            public string? MonthName { get; set; }
            public int Year { get; set; }
            public int MonthNumber { get; set; }
            public int ScheduledCount { get; set; }
            public int DeliveredCount { get; set; }
            public int CancelledCount { get; set; }
        }

        private class TrainerEngagementRow
        {
            public string? TrainerName { get; set; }
            public string? Email { get; set; }
            public int TotalTrainings { get; set; }
            public int DeliveredCount { get; set; }
            public int CancelledCount { get; set; }
            public string? CourseList { get; set; }
        }

        private class TrainingTypeRow
        {
            public string? FormatName { get; set; }
            public int Count { get; set; }
        }

        private class TrainingTypeCourseRow
        {
            public string? CourseTitle { get; set; }
            public DateTime? CourseDate { get; set; }
            public string? FormatName { get; set; }
            public string? SiteName { get; set; }
            public string? Location { get; set; }
            public bool? Delivered { get; set; }
            public bool? Cancelled { get; set; }
        }

        private class RepeatParticipantRow
        {
            public int UserSysId { get; set; }
            public string? UserName { get; set; }
            public string? Email { get; set; }
            public int TotalCoursesAttended { get; set; }
            public string? CourseList { get; set; }
        }

        private async Task<List<RepeatParticipantRow>> GetRepeatParticipantsData(
            DateTime fromDate,
            DateTime toDate)
        {
            var data = await (
    from uc in _context.UserCourses
    join c in _context.Courses on uc.CourseSysId equals c.CourseSysId

    join s in _context.Subjects on c.SubjectSysId equals s.SubjectSysId into subjectJoin
    from subject in subjectJoin.DefaultIfEmpty()

    join user in _context.Users on uc.UserSysId equals user.UserSysId into userJoin
    from user in userJoin.DefaultIfEmpty()

    where !c.Hidden
          && uc.Attended == true
          && c.CourseDate.HasValue
          && c.CourseDate.Value.Date >= fromDate.Date
          && c.CourseDate.Value.Date <= toDate.Date

    select new
    {
        uc.UserSysId,
        UserName = user != null
    ? ((user.FirstName ?? "") + " " + (user.LastName ?? "")).Trim()
    : null,
        Email = user != null ? user.Email : null,
        CourseTitle = subject != null ? subject.CourseTitle : "Untitled Course",
        c.CourseDate
    }
).ToListAsync();

            return data
                .GroupBy(x => new { x.UserSysId, x.UserName, x.Email })
                .Where(g => g.Count() > 1)
                .Select(g => new RepeatParticipantRow
                {
                    UserSysId = g.Key.UserSysId,
                    UserName = g.Key.UserName ?? "N/A",
                    Email = g.Key.Email ?? "N/A",
                    TotalCoursesAttended = g.Count(),
                    CourseList = string.Join(" | ",
                        g.OrderBy(x => x.CourseDate)
                         .Select(x => $"{x.CourseTitle} ({x.CourseDate:MM/dd/yyyy})"))
                })
                .OrderByDescending(x => x.TotalCoursesAttended)
                .ThenBy(x => x.UserName)
                .ToList();
        }

        private async Task<List<TrainingTypeCourseRow>> GetTrainingTypeCourseListData(
            DateTime fromDate,
            DateTime toDate)
        {
            return await _context.Courses
                .Include(c => c.Subject)
                .Where(c => !c.Hidden)
                .Where(c =>
                    c.CourseDate.HasValue &&
                    c.CourseDate.Value.Date >= fromDate.Date &&
                    c.CourseDate.Value.Date <= toDate.Date
                )
                .OrderBy(c => c.Format)
                .ThenBy(c => c.CourseDate)
                .Select(c => new TrainingTypeCourseRow
                {
                    CourseTitle = c.Subject != null ? c.Subject.CourseTitle : "Untitled Course",
                    CourseDate = c.CourseDate,

                    FormatName = _context.LkFormats
                        .Where(f => f.Code == c.Format)
                        .Select(f => f.Value)
                        .FirstOrDefault(),

                    SiteName = _context.Sites
                        .Where(s => s.SiteSysId == c.SiteSysId)
                        .Select(s => s.SiteName)
                        .FirstOrDefault(),

                    Location = c.TrainingLocation ?? c.City,
                    Delivered = c.Delivered,
                    Cancelled = c.Cancelled
                })
                .ToListAsync();
        }

        private async Task<List<TrainingTypeRow>> GetTrainingTypeSummaryData(
            DateTime fromDate,
            DateTime toDate)
        {
            var data = await GetTrainingTypeCourseListData(fromDate, toDate);

            return data
                .GroupBy(x => x.FormatName ?? "N/A")
                .Select(g => new TrainingTypeRow
                {
                    FormatName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.FormatName)
                .ToList();
        }

        private async Task<List<TrainerEngagementRow>> GetTrainerEngagementData(
    DateTime fromDate,
    DateTime toDate)
        {
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Where(c => !c.Hidden)
                .Where(c =>
                    c.CourseDate.HasValue &&
                    c.CourseDate.Value.Date >= fromDate.Date &&
                    c.CourseDate.Value.Date <= toDate.Date
                )
                .Select(c => new
                {
                    c.CourseSysId,
                    c.CourseDate,
                    c.Delivered,
                    c.Cancelled,
                    c.Instructor1,
                    c.Instructor2,
                    CourseTitle = c.Subject != null ? c.Subject.CourseTitle : "Untitled Course"
                })
                .ToListAsync();

            var instructors = await _context.Instructors
                .Select(i => new
                {
                    i.InstructorSysId,
                    i.Name,
                    i.Email
                })
                .ToListAsync();

            var rows = new List<dynamic>();

            foreach (var course in courses)
            {
                if (course.Instructor1.HasValue)
                {
                    rows.Add(new
                    {
                        InstructorId = course.Instructor1.Value,
                        course.CourseTitle,
                        course.Delivered,
                        course.Cancelled
                    });
                }

                if (course.Instructor2.HasValue)
                {
                    rows.Add(new
                    {
                        InstructorId = course.Instructor2.Value,
                        course.CourseTitle,
                        course.Delivered,
                        course.Cancelled
                    });
                }
            }

            var result = rows
                .GroupBy(x => x.InstructorId)
                .Select(g =>
                {
                    var trainer = instructors.FirstOrDefault(i => i.InstructorSysId == g.Key);

                    return new TrainerEngagementRow
                    {
                        TrainerName = trainer?.Name ?? "Unknown Trainer",
                        Email = trainer?.Email,
                        TotalTrainings = g.Count(),
                        DeliveredCount = g.Count(x => x.Delivered == true),
                        CancelledCount = g.Count(x => x.Cancelled == true),
                        CourseList = string.Join(" | ", g.Select(x => x.CourseTitle).Distinct())
                    };
                })
                .OrderByDescending(x => x.TotalTrainings)
                .ThenBy(x => x.TrainerName)
                .ToList();

            return result;
        }

        private async Task<List<TrainingByMonthRow>> GetTrainingByMonthData(
    DateTime fromDate,
    DateTime toDate)
        {
            var courses = await _context.Courses
                .Where(c => !c.Hidden)
                .Where(c =>
                    c.CourseDate.HasValue &&
                    c.CourseDate.Value.Date >= fromDate.Date &&
                    c.CourseDate.Value.Date <= toDate.Date
                )
                .ToListAsync();

            return courses
                .GroupBy(c => new
                {
                    c.CourseDate!.Value.Year,
                    c.CourseDate!.Value.Month
                })
                .Select(g => new TrainingByMonthRow
                {
                    Year = g.Key.Year,
                    MonthNumber = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    ScheduledCount = g.Count(),
                    DeliveredCount = g.Count(x => x.Delivered == true),
                    CancelledCount = g.Count(x => x.Cancelled == true)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.MonthNumber)
                .ToList();
        }

        private async Task<List<CancelledTrainingRow>> GetCancelledTrainingsData(
    DateTime fromDate,
    DateTime toDate)
        {
            return await _context.Courses
                .Include(c => c.Subject)
                .Where(c => !c.Hidden)
                .Where(c => c.Cancelled == true)
                .Where(c =>
                    c.CourseDate.HasValue &&
                    c.CourseDate.Value.Date >= fromDate.Date &&
                    c.CourseDate.Value.Date <= toDate.Date
                )
                .OrderBy(c => c.CourseDate)
                .Select(c => new CancelledTrainingRow
                {
                    CourseTitle = c.Subject != null
                        ? c.Subject.CourseTitle
                        : "Untitled Course",

                    CourseDate = c.CourseDate,

                    SiteName = _context.Sites
                        .Where(s => s.SiteSysId == c.SiteSysId)
                        .Select(s => s.SiteName)
                        .FirstOrDefault(),

                    FormatLabel = _context.LkFormats
                        .Where(f => f.Code == c.Format)
                        .Select(f => f.Value)
                        .FirstOrDefault(),

                    Location = c.TrainingLocation ?? c.City
                })
                .ToListAsync();
        }

        private async Task<List<CancelledByDayRow>> GetCancelledByDayData(
    DateTime fromDate,
    DateTime toDate)
        {
            var courses = await _context.Courses
                .Where(c => !c.Hidden)
                .Where(c => c.Cancelled == true)
                .Where(c =>
                    c.CourseDate.HasValue &&
                    c.CourseDate.Value.Date >= fromDate.Date &&
                    c.CourseDate.Value.Date <= toDate.Date
                )
                .ToListAsync();

            return courses
                .GroupBy(c => c.CourseDate!.Value.DayOfWeek)
                .Select(g => new CancelledByDayRow
                {
                    DayName = g.Key.ToString(),
                    DayNumber = (int)g.Key,
                    CancelledCount = g.Count()
                })
                .OrderBy(x => x.DayNumber)
                .ToList();
        }

        private async Task<List<AverageAttendanceRow>> GetAverageAttendanceData(
    DateTime fromDate,
    DateTime toDate)
        {
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Where(c => !c.Hidden)
                .Where(c => c.Delivered == true)
                .Where(c =>
                    c.CourseDate.HasValue &&
                    c.CourseDate.Value.Date >= fromDate.Date &&
                    c.CourseDate.Value.Date <= toDate.Date
                )
                .Select(c => new
                {
                    c.CourseSysId,
                    CourseTitle = c.Subject != null ? c.Subject.CourseTitle : "Untitled Course"
                })
                .ToListAsync();

            var courseIds = courses.Select(c => c.CourseSysId).ToList();

            var attendance = await _context.UserCourses
                .Where(uc => courseIds.Contains(uc.CourseSysId))
                .Where(uc => uc.Attended == true)
                .GroupBy(uc => uc.CourseSysId)
                .Select(g => new
                {
                    CourseSysId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var data = courses
                .GroupBy(c => c.CourseTitle)
                .Select(g =>
                {
                    var deliveredSessions = g.Count();

                    var totalAttendance = g.Sum(course =>
                        attendance
                            .Where(a => a.CourseSysId == course.CourseSysId)
                            .Select(a => a.Count)
                            .FirstOrDefault()
                    );

                    return new AverageAttendanceRow
                    {
                        CourseTitle = g.Key,
                        DeliveredSessions = deliveredSessions,
                        TotalAttendance = totalAttendance,
                        AverageAttendance = deliveredSessions == 0
                            ? 0
                            : Math.Round((double)totalAttendance / deliveredSessions, 2)
                    };
                })
                .OrderByDescending(x => x.AverageAttendance)
                .ThenBy(x => x.CourseTitle)
                .ToList();

            return data;
        }

        private async Task<List<PopularTrainingRow>> GetPopularTrainingTop3Data(
    DateTime fromDate,
    DateTime toDate)
        {
            var data = await (
                from uc in _context.UserCourses
                join c in _context.Courses on uc.CourseSysId equals c.CourseSysId
                join s in _context.Subjects on c.SubjectSysId equals s.SubjectSysId into subjectJoin
                from subject in subjectJoin.DefaultIfEmpty()
                where !c.Hidden
                      && uc.Attended == true
                      && c.CourseDate.HasValue
                      && c.CourseDate.Value.Date >= fromDate.Date
                      && c.CourseDate.Value.Date <= toDate.Date
                group uc by subject != null ? subject.CourseTitle : "Untitled Course" into g
                select new PopularTrainingRow
                {
                    CourseTitle = g.Key,
                    TotalAttendance = g.Count()
                }
            )
            .OrderByDescending(x => x.TotalAttendance)
            .ThenBy(x => x.CourseTitle)
            .Take(3)
            .ToListAsync();

            return data;
        }

        private async Task<List<DayOfWeekDeliveredRow>> GetDayOfWeekDeliveredData(DateTime fromDate, DateTime toDate)
        {
            var courses = await _context.Courses
                .Where(c => !c.Hidden)
                .Where(c => c.Delivered == true)
                .Where(c =>
                    c.CourseDate.HasValue &&
                    c.CourseDate.Value.Date >= fromDate.Date &&
                    c.CourseDate.Value.Date <= toDate.Date
                )
                .ToListAsync();

            return courses
                .GroupBy(c => c.CourseDate!.Value.DayOfWeek)
                .Select(g => new DayOfWeekDeliveredRow
                {
                    DayName = g.Key.ToString(),
                    DayNumber = (int)g.Key,
                    DeliveredCount = g.Count()
                })
                .OrderBy(x => x.DayNumber)
                .ToList();
        }
        private async Task<List<TrainingDeliveryFrequencyRow>> GetTrainingDeliveryFrequencyData(
    DateTime fromDate,
    DateTime toDate)
        {
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Where(c => !c.Hidden)
                .Where(c => c.Delivered == true)
                .Where(c =>
                    c.CourseDate.HasValue &&
                    c.CourseDate.Value.Date >= fromDate.Date &&
                    c.CourseDate.Value.Date <= toDate.Date
                )
                .ToListAsync();

            return courses
                .GroupBy(c => c.Subject != null ? c.Subject.CourseTitle : "Untitled Course")
                .Select(g => new TrainingDeliveryFrequencyRow
                {
                    CourseTitle = g.Key,
                    DeliveredCount = g.Count()
                })
                .OrderByDescending(x => x.DeliveredCount)
                .ThenBy(x => x.CourseTitle)
                .ToList();
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetReportCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Where(c => !c.Hidden)
                .OrderByDescending(c => c.CourseDate)
                .Select(c => new
                {
                    c.CourseSysId,
                    c.CourseDate,
                    c.EndDate,
                    c.CourseTime,
                    c.City,
                    c.TrainingLocation,
                    SubjectTitle = c.Subject != null ? c.Subject.CourseTitle : "Untitled Course",

                    SiteName = _context.Sites
                        .Where(s => s.SiteSysId == c.SiteSysId)
                        .Select(s => s.SiteName)
                        .FirstOrDefault(),

                    FormatLabel = _context.LkFormats
                        .Where(f => f.Code == c.Format)
                        .Select(f => f.Value)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(courses);
        }

        private IActionResult? ValidateDateRange(DateTime? fromDate, DateTime? toDate)
        {
            if (!fromDate.HasValue || !toDate.HasValue)
                return BadRequest(new { message = "Please select both From Date and To Date." });

            if (fromDate.Value.Date > toDate.Value.Date)
                return BadRequest(new { message = "From Date should be before To Date." });

            return null;
        }

        private async Task<List<TrainingScheduledRow>> GetTrainingsScheduledData(DateTime fromDate, DateTime toDate)
        {
            return await _context.Courses
                .Include(c => c.Subject)
                .Where(c => !c.Hidden)
                .Where(c =>
                    (c.CourseDate.HasValue && c.CourseDate.Value.Date >= fromDate.Date && c.CourseDate.Value.Date <= toDate.Date) ||
                    (c.EndDate.HasValue && c.EndDate.Value.Date >= fromDate.Date && c.EndDate.Value.Date <= toDate.Date)
                )
                .OrderBy(c => c.CourseDate)
                .Select(c => new TrainingScheduledRow
                {
                    CourseTitle = c.Subject != null ? c.Subject.CourseTitle : "Untitled Course",
                    CourseDate = c.CourseDate,
                    EndDate = c.EndDate,
                    CourseTime = c.CourseTime,
                    SiteName = _context.Sites
                        .Where(s => s.SiteSysId == c.SiteSysId)
                        .Select(s => s.SiteName)
                        .FirstOrDefault(),
                    FormatLabel = _context.LkFormats
                        .Where(f => f.Code == c.Format)
                        .Select(f => f.Value)
                        .FirstOrDefault(),
                    RegionLabel = _context.LkRegionCnties
                        .Where(r => r.Code == c.Region)
                        .Select(r => r.Value)
                        .FirstOrDefault(),
                    Location = c.TrainingLocation ?? c.City,
                    MaxSeats = c.MaxSeats,
                    Delivered = c.Delivered,
                    Cancelled = c.Cancelled
                })
                .ToListAsync();
        }
        private async Task<List<DeliveredScheduledRatioRow>> GetDeliveredScheduledRatioData(DateTime fromDate, DateTime toDate)
        {
            return await _context.Courses
                .Include(c => c.Subject)
                .Where(c => !c.Hidden)
                .Where(c =>
                    c.CourseDate.HasValue &&
                    c.CourseDate.Value.Date >= fromDate.Date &&
                    c.CourseDate.Value.Date <= toDate.Date
                )
                .OrderBy(c => c.CourseDate)
                .Select(c => new DeliveredScheduledRatioRow
                {
                    CourseTitle = c.Subject != null ? c.Subject.CourseTitle : "Untitled Course",
                    CourseDate = c.CourseDate,

                    SiteName = _context.Sites
                        .Where(s => s.SiteSysId == c.SiteSysId)
                        .Select(s => s.SiteName)
                        .FirstOrDefault(),

                    FormatLabel = _context.LkFormats
                        .Where(f => f.Code == c.Format)
                        .Select(f => f.Value)
                        .FirstOrDefault(),

                    Location = c.TrainingLocation ?? c.City,
                    Delivered = c.Delivered
                })
                .ToListAsync();
        }

        [HttpGet("delivered-scheduled-ratio/pdf")]
        public async Task<IActionResult> DownloadDeliveredScheduledRatioPdf(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetDeliveredScheduledRatioData(fromDate!.Value, toDate!.Value);

            var scheduled = data.Count;
            var delivered = data.Count(x => x.Delivered == true);
            var percent = scheduled == 0 ? 0 : Math.Round((double)delivered / scheduled * 100, 2);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Delivered-to-Scheduled Ratio Report")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text($"Date Range: {fromDate.Value:MM/dd/yyyy} - {toDate.Value:MM/dd/yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Background("#f4eff9").Padding(10).Text($"Scheduled: {scheduled}").FontSize(12).SemiBold();
                            row.RelativeItem().Background("#f4eff9").Padding(10).Text($"Delivered: {delivered}").FontSize(12).SemiBold();
                            row.RelativeItem().Background("#f4eff9").Padding(10).Text($"Delivered %: {percent}%").FontSize(12).SemiBold();
                        });

                        col.Item().PaddingTop(16).Text("Course Details")
                            .FontSize(14)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });

                            string[] headers = { "Course Title", "Date", "Site", "Format", "Location", "Delivered" };

                            table.Header(header =>
                            {
                                foreach (var h in headers)
                                {
                                    header.Cell()
                                        .Background("#43285D")
                                        .Padding(5)
                                        .Text(h)
                                        .FontColor(Colors.White)
                                        .SemiBold()
                                        .FontSize(9);
                                }
                            });

                            foreach (var rowData in data)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(rowData.CourseTitle ?? "N/A").FontSize(8);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(rowData.CourseDate?.ToString("MM/dd/yyyy") ?? "N/A").FontSize(8);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(rowData.SiteName ?? "N/A").FontSize(8);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(rowData.FormatLabel ?? "N/A").FontSize(8);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(rowData.Location ?? "N/A").FontSize(8);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(rowData.Delivered == true ? "Yes" : "No").FontSize(8);
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Delivered_Scheduled_Ratio_Report.pdf");
        }

        [HttpGet("delivered-scheduled-ratio/csv")]
        public async Task<IActionResult> DownloadDeliveredScheduledRatioCsv(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetDeliveredScheduledRatioData(fromDate!.Value, toDate!.Value);

            var scheduled = data.Count;
            var delivered = data.Count(x => x.Delivered == true);
            var percent = scheduled == 0 ? 0 : Math.Round((double)delivered / scheduled * 100, 2);

            var csv = new StringBuilder();

            csv.AppendLine("Summary");
            csv.AppendLine("Scheduled,Delivered,Delivered %");
            csv.AppendLine($"{scheduled},{delivered},{percent}%");
            csv.AppendLine();

            csv.AppendLine("Course Details");
            csv.AppendLine("Course Title,Date,Site,Format,Location,Delivered");

            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.CourseTitle),
                    EscapeCsv(row.CourseDate?.ToString("MM/dd/yyyy")),
                    EscapeCsv(row.SiteName),
                    EscapeCsv(row.FormatLabel),
                    EscapeCsv(row.Location),
                    EscapeCsv(row.Delivered == true ? "Yes" : "No")
                ));
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Delivered_Scheduled_Ratio_Report.csv");
        }

        [HttpGet("delivered-scheduled-ratio/chart")]
        public async Task<IActionResult> GetDeliveredScheduledRatioChart(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetDeliveredScheduledRatioData(fromDate!.Value, toDate!.Value);

            var scheduled = data.Count;
            var delivered = data.Count(x => x.Delivered == true);
            var percent = scheduled == 0 ? 0 : Math.Round((double)delivered / scheduled * 100, 2);

            return Ok(new
            {
                scheduled,
                delivered,
                deliveredPercent = percent
            });
        }

        [HttpGet("day-of-week-delivered/pdf")]
        public async Task<IActionResult> DownloadDayOfWeekDeliveredPdf(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetDayOfWeekDeliveredData(fromDate!.Value, toDate!.Value);
            var totalDelivered = data.Sum(x => x.DeliveredCount);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Breakdown by Day of the Week - Delivered Only")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text($"Date Range: {fromDate.Value:MM/dd/yyyy} - {toDate.Value:MM/dd/yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Column(col =>
                    {
                        col.Item()
                            .Background("#f4eff9")
                            .Padding(10)
                            .Text($"Total Delivered Trainings: {totalDelivered}")
                            .FontSize(12)
                            .SemiBold();

                        col.Item().PaddingTop(14).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            string[] headers = { "Day of Week", "Delivered Count" };

                            table.Header(header =>
                            {
                                foreach (var h in headers)
                                {
                                    header.Cell()
                                        .Background("#43285D")
                                        .Padding(6)
                                        .Text(h)
                                        .FontColor(Colors.White)
                                        .SemiBold()
                                        .FontSize(10);
                                }
                            });

                            foreach (var row in data)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.DayName ?? "N/A").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.DeliveredCount.ToString()).FontSize(9);
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Delivered_By_Day_Of_Week_Report.pdf");
        }

        [HttpGet("day-of-week-delivered/csv")]
        public async Task<IActionResult> DownloadDayOfWeekDeliveredCsv(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetDayOfWeekDeliveredData(fromDate!.Value, toDate!.Value);

            var csv = new StringBuilder();
            csv.AppendLine("Day of Week,Delivered Count");

            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.DayName),
                    EscapeCsv(row.DeliveredCount.ToString())
                ));
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Delivered_By_Day_Of_Week_Report.csv");
        }

        [HttpGet("day-of-week-delivered/chart")]
        public async Task<IActionResult> GetDayOfWeekDeliveredChart(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetDayOfWeekDeliveredData(fromDate!.Value, toDate!.Value);

            return Ok(data.Select(x => new
            {
                day = x.DayName,
                count = x.DeliveredCount
            }));
        }


        [HttpGet("trainings-scheduled/pdf")]
        public async Task<IActionResult> DownloadTrainingsScheduledPdf(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetTrainingsScheduledData(fromDate!.Value, toDate!.Value);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Trainings Scheduled Report")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text($"Date Range: {fromDate.Value:MM/dd/yyyy} - {toDate.Value:MM/dd/yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.3f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            string[] headers = { "Course Title", "Start Date", "End Date", "Site", "Format", "Location", "Seats" };

                            foreach (var h in headers)
                            {
                                header.Cell()
                                    .Background("#43285D")
                                    .Padding(5)
                                    .Text(h)
                                    .FontColor(Colors.White)
                                    .SemiBold()
                                    .FontSize(9);
                            }
                        });

                        foreach (var row in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.CourseTitle).FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.CourseDate?.ToString("MM/dd/yyyy") ?? "N/A").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.EndDate?.ToString("MM/dd/yyyy") ?? "N/A").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.SiteName ?? "N/A").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.FormatLabel ?? "N/A").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.Location ?? "N/A").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.MaxSeats?.ToString() ?? "N/A").FontSize(8);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"));
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Trainings_Scheduled_Report.pdf");
        }

        private IActionResult? ValidateCourseSelection(string? courseIds)
        {
            if (string.IsNullOrWhiteSpace(courseIds))
                return BadRequest(new { message = "Please select at least one course." });

            return null;
        }

        private static List<int> ParseCourseIds(string? courseIds)
        {
            return courseIds?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => int.TryParse(x, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList() ?? new List<int>();
        }

        private async Task<List<AttendanceParticipationRow>> GetAttendanceParticipationData(List<int> courseIds)
        {
            return await _context.Courses
                .Include(c => c.Subject)
                .Where(c => courseIds.Contains(c.CourseSysId))
                .OrderBy(c => c.CourseDate)
                .Select(c => new AttendanceParticipationRow
                {
                    CourseTitle = c.Subject != null ? c.Subject.CourseTitle : "Untitled Course",
                    CourseDate = c.CourseDate,
                    SiteName = _context.Sites
                        .Where(s => s.SiteSysId == c.SiteSysId)
                        .Select(s => s.SiteName)
                        .FirstOrDefault(),
                    FormatLabel = _context.LkFormats
                        .Where(f => f.Code == c.Format)
                        .Select(f => f.Value)
                        .FirstOrDefault(),
                    MaxSeats = c.MaxSeats,

                    RegisteredCount = _context.UserCourses.Count(uc =>
                        uc.CourseSysId == c.CourseSysId &&
                        uc.Status == 1 &&
                        uc.IsWaitlisted != true
                    ),

                    AttendedCount = _context.UserCourses.Count(uc =>
                        uc.CourseSysId == c.CourseSysId &&
                        uc.Attended == true
                    )
                })
                .ToListAsync();
        }
        [HttpGet("attendance-participation/pdf")]
        public async Task<IActionResult> DownloadAttendanceParticipationPdf([FromQuery] string? courseIds)
        {
            var validation = ValidateCourseSelection(courseIds);
            if (validation != null) return validation;

            var ids = ParseCourseIds(courseIds);
            var data = await GetAttendanceParticipationData(ids);

            QuestPDF.Settings.License = LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Text("Attendance & Participation Report")
                        .FontSize(20)
                        .SemiBold()
                        .FontColor("#43285D");

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        string[] headers =
                        {
                    "Course Title", "Date", "Site", "Format", "Seats",
                    "Registered", "Attended", "Attendance %"
                };

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell()
                                    .Background("#43285D")
                                    .Padding(5)
                                    .Text(h)
                                    .FontColor(Colors.White)
                                    .SemiBold()
                                    .FontSize(9);
                            }
                        });

                        foreach (var row in data)
                        {
                            var percent = row.RegisteredCount == 0
                                ? 0
                                : Math.Round((double)row.AttendedCount / row.RegisteredCount * 100, 2);

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.CourseTitle).FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.CourseDate?.ToString("MM/dd/yyyy") ?? "N/A").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.SiteName ?? "N/A").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.FormatLabel ?? "N/A").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.MaxSeats?.ToString() ?? "N/A").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.RegisteredCount.ToString()).FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.AttendedCount.ToString()).FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{percent}%").FontSize(8);
                        }
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Attendance_Participation_Report.pdf");
        }

        [HttpGet("attendance-participation/csv")]
        public async Task<IActionResult> DownloadAttendanceParticipationCsv([FromQuery] string? courseIds)
        {
            var validation = ValidateCourseSelection(courseIds);
            if (validation != null) return validation;

            var ids = ParseCourseIds(courseIds);
            var data = await GetAttendanceParticipationData(ids);

            var csv = new StringBuilder();
            csv.AppendLine("Course Title,Date,Site,Format,Max Seats,Registered,Attended,Not Attended,Attendance %");

            foreach (var row in data)
            {
                var notAttended = row.RegisteredCount - row.AttendedCount;
                var percent = row.RegisteredCount == 0
                    ? 0
                    : Math.Round((double)row.AttendedCount / row.RegisteredCount * 100, 2);

                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.CourseTitle),
                    EscapeCsv(row.CourseDate?.ToString("MM/dd/yyyy")),
                    EscapeCsv(row.SiteName),
                    EscapeCsv(row.FormatLabel),
                    EscapeCsv(row.MaxSeats?.ToString()),
                    EscapeCsv(row.RegisteredCount.ToString()),
                    EscapeCsv(row.AttendedCount.ToString()),
                    EscapeCsv(notAttended.ToString()),
                    EscapeCsv($"{percent}%")
                ));
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Attendance_Participation_Report.csv");
        }

        [HttpGet("attendance-participation/chart")]
        public async Task<IActionResult> GetAttendanceParticipationChart([FromQuery] string? courseIds)
        {
            var validation = ValidateCourseSelection(courseIds);
            if (validation != null) return validation;

            var ids = ParseCourseIds(courseIds);
            var data = await GetAttendanceParticipationData(ids);

            var result = data.Select(x => new
            {
                courseTitle = x.CourseTitle,
                registered = x.RegisteredCount,
                attended = x.AttendedCount
            });

            return Ok(result);
        }

        [HttpGet("trainings-scheduled/chart")]
        public async Task<IActionResult> GetTrainingsScheduledChart(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await _context.Courses
                .Where(c => !c.Hidden)
                .Where(c =>
                    c.CourseDate.HasValue &&
                    c.CourseDate.Value.Date >= fromDate!.Value.Date &&
                    c.CourseDate.Value.Date <= toDate!.Value.Date
                )
                .GroupBy(c => new
                {
                    c.CourseDate!.Value.Year,
                    c.CourseDate!.Value.Month
                })
                .Select(g => new
                {
                    year = g.Key.Year,
                    monthNumber = g.Key.Month,
                    count = g.Count()
                })
                .OrderBy(x => x.year)
                .ThenBy(x => x.monthNumber)
                .ToListAsync();

            var result = data.Select(x => new
            {
                month = new DateTime(x.year, x.monthNumber, 1).ToString("MMM yyyy"),
                count = x.count
            });

            return Ok(result);
        }

        [HttpGet("trainings-scheduled/csv")]
        public async Task<IActionResult> DownloadTrainingsScheduledCsv(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetTrainingsScheduledData(fromDate!.Value, toDate!.Value);

            var csv = new StringBuilder();

            csv.AppendLine("Course Title,Start Date,End Date,Time,Site,Format,Region,Location,Max Seats,Delivered,Cancelled");

            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.CourseTitle),
                    EscapeCsv(row.CourseDate?.ToString("MM/dd/yyyy")),
                    EscapeCsv(row.EndDate?.ToString("MM/dd/yyyy")),
                    EscapeCsv(row.CourseTime),
                    EscapeCsv(row.SiteName),
                    EscapeCsv(row.FormatLabel),
                    EscapeCsv(row.RegionLabel),
                    EscapeCsv(row.Location),
                    EscapeCsv(row.MaxSeats?.ToString()),
                    EscapeCsv(row.Delivered == true ? "Yes" : "No"),
                    EscapeCsv(row.Cancelled == true ? "Yes" : "No")
                ));
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "Trainings_Scheduled_Report.csv");
        }

        [HttpGet("training-delivery-frequency/pdf")]
        public async Task<IActionResult> DownloadTrainingDeliveryFrequencyPdf(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetTrainingDeliveryFrequencyData(fromDate!.Value, toDate!.Value);
            var totalDelivered = data.Sum(x => x.DeliveredCount);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Training Delivery by Frequency")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text($"Date Range: {fromDate.Value:MM/dd/yyyy} - {toDate.Value:MM/dd/yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Column(col =>
                    {
                        col.Item()
                            .Background("#f4eff9")
                            .Padding(10)
                            .Text($"Total Delivered Trainings: {totalDelivered}")
                            .FontSize(12)
                            .SemiBold();

                        col.Item().PaddingTop(14).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(1);
                            });

                            string[] headers = { "Training Name", "Delivered Count" };

                            table.Header(header =>
                            {
                                foreach (var h in headers)
                                {
                                    header.Cell()
                                        .Background("#43285D")
                                        .Padding(6)
                                        .Text(h)
                                        .FontColor(Colors.White)
                                        .SemiBold()
                                        .FontSize(10);
                                }
                            });

                            foreach (var row in data)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.CourseTitle ?? "N/A").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.DeliveredCount.ToString()).FontSize(9);
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Training_Delivery_Frequency_Report.pdf");
        }

        [HttpGet("training-delivery-frequency/csv")]
        public async Task<IActionResult> DownloadTrainingDeliveryFrequencyCsv(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetTrainingDeliveryFrequencyData(fromDate!.Value, toDate!.Value);

            var csv = new StringBuilder();
            csv.AppendLine("Training Name,Delivered Count");

            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.CourseTitle),
                    EscapeCsv(row.DeliveredCount.ToString())
                ));
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Training_Delivery_Frequency_Report.csv");
        }

        [HttpGet("training-delivery-frequency/chart")]
        public async Task<IActionResult> GetTrainingDeliveryFrequencyChart(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetTrainingDeliveryFrequencyData(fromDate!.Value, toDate!.Value);

            return Ok(data.Select(x => new
            {
                training = x.CourseTitle,
                count = x.DeliveredCount
            }));
        }

        [HttpGet("popular-training-top3/pdf")]
        public async Task<IActionResult> DownloadPopularTrainingTop3Pdf(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetPopularTrainingTop3Data(fromDate!.Value, toDate!.Value);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Most Popular Training by Total Attendance - Top 3")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text($"Date Range: {fromDate.Value:MM/dd/yyyy} - {toDate.Value:MM/dd/yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                        });

                        string[] headers = { "Rank", "Training Name", "Total Attendance" };

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell()
                                    .Background("#43285D")
                                    .Padding(6)
                                    .Text(h)
                                    .FontColor(Colors.White)
                                    .SemiBold()
                                    .FontSize(10);
                            }
                        });

                        var rank = 1;

                        foreach (var row in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(rank.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.CourseTitle ?? "N/A").FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.TotalAttendance.ToString()).FontSize(9);

                            rank++;
                        }
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Popular_Training_Top3_Report.pdf");
        }

        [HttpGet("popular-training-top3/csv")]
        public async Task<IActionResult> DownloadPopularTrainingTop3Csv(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetPopularTrainingTop3Data(fromDate!.Value, toDate!.Value);

            var csv = new StringBuilder();
            csv.AppendLine("Rank,Training Name,Total Attendance");

            var rank = 1;

            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(rank.ToString()),
                    EscapeCsv(row.CourseTitle),
                    EscapeCsv(row.TotalAttendance.ToString())
                ));

                rank++;
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Popular_Training_Top3_Report.csv");
        }

        [HttpGet("popular-training-top3/chart")]
        public async Task<IActionResult> GetPopularTrainingTop3Chart(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetPopularTrainingTop3Data(fromDate!.Value, toDate!.Value);

            return Ok(data.Select(x => new
            {
                training = x.CourseTitle,
                attendance = x.TotalAttendance
            }));
        }

        [HttpGet("average-attendance/pdf")]
        public async Task<IActionResult> DownloadAverageAttendancePdf(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetAverageAttendanceData(fromDate!.Value, toDate!.Value);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Top Trainings by Average Attendance")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text($"Date Range: {fromDate.Value:MM/dd/yyyy} - {toDate.Value:MM/dd/yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        string[] headers =
                        {
                    "Training Name", "Delivered Sessions", "Total Attendance", "Average Attendance"
                };

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell()
                                    .Background("#43285D")
                                    .Padding(6)
                                    .Text(h)
                                    .FontColor(Colors.White)
                                    .SemiBold()
                                    .FontSize(10);
                            }
                        });

                        foreach (var row in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.CourseTitle ?? "N/A").FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.DeliveredSessions.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.TotalAttendance.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.AverageAttendance.ToString()).FontSize(9);
                        }
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Average_Attendance_Report.pdf");
        }
        [HttpGet("average-attendance/csv")]
        public async Task<IActionResult> DownloadAverageAttendanceCsv(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetAverageAttendanceData(fromDate!.Value, toDate!.Value);

            var csv = new StringBuilder();
            csv.AppendLine("Training Name,Delivered Sessions,Total Attendance,Average Attendance");

            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.CourseTitle),
                    EscapeCsv(row.DeliveredSessions.ToString()),
                    EscapeCsv(row.TotalAttendance.ToString()),
                    EscapeCsv(row.AverageAttendance.ToString())
                ));
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Average_Attendance_Report.csv");
        }
        [HttpGet("average-attendance/chart")]
        public async Task<IActionResult> GetAverageAttendanceChart(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetAverageAttendanceData(fromDate!.Value, toDate!.Value);

            return Ok(data.Select(x => new
            {
                training = x.CourseTitle,
                average = x.AverageAttendance
            }));
        }

        [HttpGet("cancelled-by-day/pdf")]
        public async Task<IActionResult> DownloadCancelledByDayPdf(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetCancelledByDayData(fromDate!.Value, toDate!.Value);
            var totalCancelled = data.Sum(x => x.CancelledCount);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Cancelled Trainings by Day of the Week")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text($"Date Range: {fromDate.Value:MM/dd/yyyy} - {toDate.Value:MM/dd/yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Column(col =>
                    {
                        col.Item()
                            .Background("#f4eff9")
                            .Padding(10)
                            .Text($"Total Cancelled Trainings: {totalCancelled}")
                            .FontSize(12)
                            .SemiBold();

                        col.Item().PaddingTop(14).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            string[] headers = { "Day of Week", "Cancelled Count" };

                            table.Header(header =>
                            {
                                foreach (var h in headers)
                                {
                                    header.Cell()
                                        .Background("#43285D")
                                        .Padding(6)
                                        .Text(h)
                                        .FontColor(Colors.White)
                                        .SemiBold()
                                        .FontSize(10);
                                }
                            });

                            foreach (var row in data)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.DayName ?? "N/A").FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.CancelledCount.ToString()).FontSize(9);
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Cancelled_By_Day_Report.pdf");
        }

        [HttpGet("cancelled-by-day/csv")]
        public async Task<IActionResult> DownloadCancelledByDayCsv(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetCancelledByDayData(fromDate!.Value, toDate!.Value);

            var csv = new StringBuilder();
            csv.AppendLine("Day of Week,Cancelled Count");

            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.DayName),
                    EscapeCsv(row.CancelledCount.ToString())
                ));
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Cancelled_By_Day_Report.csv");
        }

        [HttpGet("cancelled-by-day/chart")]
        public async Task<IActionResult> GetCancelledByDayChart(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetCancelledByDayData(fromDate!.Value, toDate!.Value);

            return Ok(data.Select(x => new
            {
                day = x.DayName,
                count = x.CancelledCount
            }));
        }

        [HttpGet("cancelled-trainings/pdf")]
        public async Task<IActionResult> DownloadCancelledTrainingsPdf(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetCancelledTrainingsData(
                fromDate!.Value,
                toDate!.Value);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Cancelled Trainings Report")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text(
                            $"Date Range: {fromDate:MM/dd/yyyy} - {toDate:MM/dd/yyyy}")
                            .FontSize(10);
                    });

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        string[] headers =
                        {
                    "Course Title",
                    "Date",
                    "Site",
                    "Format",
                    "Location"
                };

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell()
                                    .Background("#43285D")
                                    .Padding(5)
                                    .Text(h)
                                    .FontColor(Colors.White)
                                    .SemiBold();
                            }
                        });

                        foreach (var row in data)
                        {
                            table.Cell().Padding(5)
                                .Text(row.CourseTitle ?? "N/A");

                            table.Cell().Padding(5)
                                .Text(row.CourseDate?.ToString("MM/dd/yyyy") ?? "N/A");

                            table.Cell().Padding(5)
                                .Text(row.SiteName ?? "N/A");

                            table.Cell().Padding(5)
                                .Text(row.FormatLabel ?? "N/A");

                            table.Cell().Padding(5)
                                .Text(row.Location ?? "N/A");
                        }
                    });
                });
            }).GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                "Cancelled_Trainings_Report.pdf");
        }
        [HttpGet("cancelled-trainings/csv")]
        public async Task<IActionResult> DownloadCancelledTrainingsCsv(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetCancelledTrainingsData(
                fromDate!.Value,
                toDate!.Value);

            var csv = new StringBuilder();

            csv.AppendLine(
                "Course Title,Date,Site,Format,Location");

            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.CourseTitle),
                    EscapeCsv(row.CourseDate?.ToString("MM/dd/yyyy")),
                    EscapeCsv(row.SiteName),
                    EscapeCsv(row.FormatLabel),
                    EscapeCsv(row.Location)
                ));
            }

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "Cancelled_Trainings_Report.csv");
        }

        [HttpGet("training-by-month/pdf")]
        public async Task<IActionResult> DownloadTrainingByMonthPdf(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetTrainingByMonthData(fromDate!.Value, toDate!.Value);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Training by Month")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text($"Date Range: {fromDate.Value:MM/dd/yyyy} - {toDate.Value:MM/dd/yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        string[] headers =
                        {
                    "Month",
                    "Scheduled",
                    "Delivered",
                    "Cancelled"
                };

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell()
                                    .Background("#43285D")
                                    .Padding(6)
                                    .Text(h)
                                    .FontColor(Colors.White)
                                    .SemiBold()
                                    .FontSize(10);
                            }
                        });

                        foreach (var row in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.MonthName ?? "N/A").FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.ScheduledCount.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.DeliveredCount.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(row.CancelledCount.ToString()).FontSize(9);
                        }
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Training_By_Month_Report.pdf");
        }

        [HttpGet("training-by-month/csv")]
        public async Task<IActionResult> DownloadTrainingByMonthCsv(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetTrainingByMonthData(fromDate!.Value, toDate!.Value);

            var csv = new StringBuilder();
            csv.AppendLine("Month,Scheduled,Delivered,Cancelled");

            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.MonthName),
                    EscapeCsv(row.ScheduledCount.ToString()),
                    EscapeCsv(row.DeliveredCount.ToString()),
                    EscapeCsv(row.CancelledCount.ToString())
                ));
            }

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "Training_By_Month_Report.csv");
        }

        [HttpGet("training-by-month/chart")]
        public async Task<IActionResult> GetTrainingByMonthChart(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetTrainingByMonthData(fromDate!.Value, toDate!.Value);

            return Ok(data.Select(x => new
            {
                month = x.MonthName,
                scheduled = x.ScheduledCount,
                delivered = x.DeliveredCount,
                cancelled = x.CancelledCount
            }));
        }

        [HttpGet("trainer-engagement/pdf")]
        public async Task<IActionResult> DownloadTrainerEngagementPdf(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetTrainerEngagementData(fromDate!.Value, toDate!.Value);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Trainer Engagement Report")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text($"Date Range: {fromDate.Value:MM/dd/yyyy} - {toDate.Value:MM/dd/yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(4);
                        });

                        string[] headers =
                        {
                    "Trainer",
                    "Email",
                    "Total",
                    "Delivered",
                    "Cancelled",
                    "Courses"
                };

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell()
                                    .Background("#43285D")
                                    .Padding(5)
                                    .Text(h)
                                    .FontColor(Colors.White)
                                    .SemiBold()
                                    .FontSize(9);
                            }
                        });

                        foreach (var row in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.TrainerName ?? "N/A").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.Email ?? "N/A").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.TotalTrainings.ToString()).FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.DeliveredCount.ToString()).FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.CancelledCount.ToString()).FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(row.CourseList ?? "N/A").FontSize(7);
                        }
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Trainer_Engagement_Report.pdf");
        }
        [HttpGet("trainer-engagement/csv")]
        public async Task<IActionResult> DownloadTrainerEngagementCsv(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetTrainerEngagementData(fromDate!.Value, toDate!.Value);

            var csv = new StringBuilder();

            csv.AppendLine("Trainer,Email,Total Trainings,Delivered,Cancelled,Courses");

            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.TrainerName),
                    EscapeCsv(row.Email),
                    EscapeCsv(row.TotalTrainings.ToString()),
                    EscapeCsv(row.DeliveredCount.ToString()),
                    EscapeCsv(row.CancelledCount.ToString()),
                    EscapeCsv(row.CourseList)
                ));
            }

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "Trainer_Engagement_Report.csv");
        }
        [HttpGet("trainer-engagement/chart")]
        public async Task<IActionResult> GetTrainerEngagementChart(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetTrainerEngagementData(fromDate!.Value, toDate!.Value);

            var top10 = data
                .OrderByDescending(x => x.TotalTrainings)
                .Take(10)
                .Select(x => new
                {
                    trainer = x.TrainerName,
                    total = x.TotalTrainings,
                    delivered = x.DeliveredCount,
                    cancelled = x.CancelledCount
                });

            return Ok(top10);
        }

        [HttpGet("training-type/chart")]
        public async Task<IActionResult> GetTrainingTypeChart(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetTrainingTypeSummaryData(fromDate!.Value, toDate!.Value);

            return Ok(data.Select(x => new
            {
                type = x.FormatName,
                count = x.Count
            }));
        }

        [HttpGet("training-type/csv")]
        public async Task<IActionResult> DownloadTrainingTypeCsv(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetTrainingTypeCourseListData(fromDate!.Value, toDate!.Value);
            var summary = await GetTrainingTypeSummaryData(fromDate.Value, toDate.Value);

            var totalTrainings = data.Count;
            var totalDelivered = data.Count(x => x.Delivered == true);
            var totalCancelled = data.Count(x => x.Cancelled == true);

            var csv = new StringBuilder();

            csv.AppendLine("Summary");
            csv.AppendLine("Total Trainings,Delivered,Cancelled");
            csv.AppendLine($"{totalTrainings},{totalDelivered},{totalCancelled}");
            csv.AppendLine();

            csv.AppendLine("Training Type Totals");
            csv.AppendLine("Training Type,Count");

            foreach (var row in summary)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.FormatName),
                    EscapeCsv(row.Count.ToString())
                ));
            }

            csv.AppendLine();

            csv.AppendLine("Course Details");
            csv.AppendLine("Course Title,Date,Training Type,Site,Location,Delivered,Cancelled");

            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsv(row.CourseTitle),
                    EscapeCsv(row.CourseDate?.ToString("MM/dd/yyyy")),
                    EscapeCsv(row.FormatName),
                    EscapeCsv(row.SiteName),
                    EscapeCsv(row.Location),
                    EscapeCsv(row.Delivered == true ? "Yes" : "No"),
                    EscapeCsv(row.Cancelled == true ? "Yes" : "No")
                ));
            }

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "Training_Type_Report.csv");
        }

        [HttpGet("training-type/pdf")]
        public async Task<IActionResult> DownloadTrainingTypePdf(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetTrainingTypeCourseListData(fromDate!.Value, toDate!.Value);
            var summary = await GetTrainingTypeSummaryData(fromDate.Value, toDate.Value);

            var totalTrainings = data.Count;
            var totalDelivered = data.Count(x => x.Delivered == true);
            var totalCancelled = data.Count(x => x.Cancelled == true);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Type of Training Report")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text($"Date Range: {fromDate.Value:MM/dd/yyyy} - {toDate.Value:MM/dd/yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Background("#f4eff9").Padding(10)
                                .Text($"Total Trainings: {totalTrainings}")
                                .FontSize(12).SemiBold();

                            row.RelativeItem().Background("#f4eff9").Padding(10)
                                .Text($"Delivered: {totalDelivered}")
                                .FontSize(12).SemiBold();

                            row.RelativeItem().Background("#f4eff9").Padding(10)
                                .Text($"Cancelled: {totalCancelled}")
                                .FontSize(12).SemiBold();
                        });

                        col.Item().PaddingTop(14).Text("Training Type Totals")
                            .FontSize(14)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            string[] headers = { "Training Type", "Count" };

                            table.Header(header =>
                            {
                                foreach (var h in headers)
                                {
                                    header.Cell()
                                        .Background("#43285D")
                                        .Padding(6)
                                        .Text(h)
                                        .FontColor(Colors.White)
                                        .SemiBold()
                                        .FontSize(10);
                                }
                            });

                            foreach (var row in summary)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                    .Text(row.FormatName ?? "N/A")
                                    .FontSize(9);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                    .Text(row.Count.ToString())
                                    .FontSize(9);
                            }
                        });

                        col.Item().PaddingTop(16).Text("Course Details")
                            .FontSize(14)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            string[] headers =
                            {
                        "Course Title", "Date", "Training Type", "Site",
                        "Location", "Delivered", "Cancelled"
                    };

                            table.Header(header =>
                            {
                                foreach (var h in headers)
                                {
                                    header.Cell()
                                        .Background("#43285D")
                                        .Padding(5)
                                        .Text(h)
                                        .FontColor(Colors.White)
                                        .SemiBold()
                                        .FontSize(9);
                                }
                            });

                            foreach (var row in data)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(row.CourseTitle ?? "N/A").FontSize(8);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(row.CourseDate?.ToString("MM/dd/yyyy") ?? "N/A").FontSize(8);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(row.FormatName ?? "N/A").FontSize(8);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(row.SiteName ?? "N/A").FontSize(8);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(row.Location ?? "N/A").FontSize(8);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(row.Delivered == true ? "Yes" : "No").FontSize(8);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(row.Cancelled == true ? "Yes" : "No").FontSize(8);
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Training_Type_Report.pdf");
        }

        [HttpGet("repeat-participants/chart")]
        public async Task<IActionResult> GetRepeatParticipantsChart(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetRepeatParticipantsData(fromDate!.Value, toDate!.Value);

            return Ok(data.Take(10).Select(x => new
            {
                participant = x.UserName ?? $"User {x.UserSysId}",
                count = x.TotalCoursesAttended
            }));
        }

        [HttpGet("repeat-participants/csv")]
        public async Task<IActionResult> DownloadRepeatParticipantsCsv(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            var data = await GetRepeatParticipantsData(fromDate!.Value, toDate!.Value);

            var totalRepeatParticipants = data.Count;
            var totalRepeatAttendances = data.Sum(x => x.TotalCoursesAttended);

            var csv = new StringBuilder();

            csv.AppendLine("Summary");
            csv.AppendLine("Repeat Participants,Total Repeat Attendances");
            csv.AppendLine($"{totalRepeatParticipants},{totalRepeatAttendances}");
            csv.AppendLine();

            csv.AppendLine("Participant Details");
            csv.AppendLine("UserSysID,UserName,Email,Total Courses Attended,Courses");
            foreach (var row in data)
            {
                csv.AppendLine(string.Join(",",
    EscapeCsv(row.UserSysId.ToString()),
    EscapeCsv(row.UserName),
    EscapeCsv(row.Email),
    EscapeCsv(row.TotalCoursesAttended.ToString()),
    EscapeCsv(row.CourseList)
));
            }

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "Repeat_Participants_Report.csv");
        }

        [HttpGet("repeat-participants/pdf")]
        public async Task<IActionResult> DownloadRepeatParticipantsPdf(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var validation = ValidateDateRange(fromDate, toDate);
            if (validation != null) return validation;

            QuestPDF.Settings.License = LicenseType.Community;

            var data = await GetRepeatParticipantsData(fromDate!.Value, toDate!.Value);

            var totalRepeatParticipants = data.Count;
            var totalRepeatAttendances = data.Sum(x => x.TotalCoursesAttended);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Repeat Participants Report")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor("#43285D");

                        col.Item().Text($"Date Range: {fromDate.Value:MM/dd/yyyy} - {toDate.Value:MM/dd/yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(15).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Background("#f4eff9").Padding(10)
                                .Text($"Repeat Participants: {totalRepeatParticipants}")
                                .FontSize(12).SemiBold();

                            row.RelativeItem().Background("#f4eff9").Padding(10)
                                .Text($"Total Repeat Attendances: {totalRepeatAttendances}")
                                .FontSize(12).SemiBold();
                        });

                        col.Item().PaddingTop(14).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(5);
                            });

                            string[] headers =
{
    "UserSysID",
    "UserName",
    "Email",
    "Courses Attended",
    "Courses"
};
                            table.Header(header =>
                            {
                                foreach (var h in headers)
                                {
                                    header.Cell()
                                        .Background("#43285D")
                                        .Padding(5)
                                        .Text(h)
                                        .FontColor(Colors.White)
                                        .SemiBold()
                                        .FontSize(9);
                                }
                            });

                            foreach (var row in data)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
    .Text(row.UserSysId.ToString()).FontSize(8);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(row.UserName ?? "N/A").FontSize(8);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(row.Email ?? "N/A").FontSize(8);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(row.TotalCoursesAttended.ToString()).FontSize(8);

                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                    .Text(row.CourseList ?? "N/A").FontSize(7);
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Repeat_Participants_Report.pdf");
        }


        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";

            value = value.Replace("\"", "\"\"");

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value}\"";

            return value;
        }

        private class TrainingScheduledRow
        {
            public string? CourseTitle { get; set; }
            public DateTime? CourseDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string? CourseTime { get; set; }
            public string? SiteName { get; set; }
            public string? FormatLabel { get; set; }
            public string? RegionLabel { get; set; }
            public string? Location { get; set; }
            public int? MaxSeats { get; set; }
            public bool? Delivered { get; set; }
            public bool? Cancelled { get; set; }
        }
    }
}
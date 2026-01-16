using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HIVTraining_Vue.Server.Requests;

namespace HIVTraining_Vue.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateCourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CreateCourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all lookup data for the course scheduling dropdowns
        /// </summary>
        [HttpGet("lookup")]
        public async Task<IActionResult> GetLookupData()
        {
            var trainingCenters = await _context.Sites
                .Where(s => s.Active)
                .Select(s => new { s.SiteSysId, s.SiteName })
                .ToListAsync();

            var regions = await _context.LkRegionCnties
                .Select(r => new { r.Code, r.Value })
                .ToListAsync();

            // ✅ Topics lookup
            var topics = await _context.LkTopics
                .OrderBy(t => t.SortKey)
                .Select(t => new { t.Code, t.Value })
                .ToListAsync();

            var instructors = await _context.Instructors
                .Where(i => i.Active == true)
                .Select(i => new { i.InstructorSysId, i.Name })
                .ToListAsync();

            var deliverables = await _context.LkDeliverables
                .Select(d => new { d.Id, d.Value })
                .ToListAsync();

            var formats = await _context.LkFormats
                .Select(f => new { f.Code, f.Value })
                .ToListAsync();

            return Ok(new
            {
                TrainingCenters = trainingCenters,
                Regions = regions,
                Topics = topics,              // ✅ return topics
                Instructors = instructors,
                Deliverables = deliverables,
                Formats = formats
            });
        }

        public class TopicFilterRequest
        {
            public List<int> TopicCodes { get; set; } = new();
        }

        [HttpGet("topicsBySubject/{subjectSysId}")]
        public async Task<IActionResult> GetTopicsBySubject(int subjectSysId)
        {
            // Using SubjectTopics join table
            var topicCodes = await _context.SubjectTopics
                .Where(x => x.SubjectSysId == subjectSysId)
                .Select(x => x.TopicCode)
                .Distinct()
                .ToListAsync();

            return Ok(topicCodes);
        }


        [HttpPost("subjectsByTopics")]
        public async Task<IActionResult> GetSubjectsByTopics([FromBody] TopicFilterRequest req)
        {
            var topicCodes = (req?.TopicCodes ?? new List<int>())
                .Distinct()
                .ToList();

            if (topicCodes.Count == 0)
                return Ok(new List<object>());

            // ✅ ANY selected topic -> include that title
            // ✅ BUT exclude online titles (already auto-created in Courses)
            var subjects = await (
                from st in _context.SubjectTopics
                join s in _context.Subjects on st.SubjectSysId equals s.SubjectSysId
                where s.Active
                      && !s.IsOnlineTraining              // ✅ EXCLUDE ONLINE TITLES
                      && topicCodes.Contains(st.TopicCode)
                select new { s.SubjectSysId, s.CourseTitle }
            )
            .Distinct()
            .OrderBy(x => x.CourseTitle)
            .ToListAsync();

            return Ok(subjects);
        }

        /// <summary>
        /// Schedule a new course
        /// </summary>
        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleCourse([FromBody] CourseScheduleRequest request)
        {
            if (request?.Course == null)
                return BadRequest("Course data is required.");

            var course = request.Course;
            course.DateEntered = DateTime.UtcNow;
            course.DateModified = DateTime.UtcNow;
            course.MarkAsNewUntil = course.MarkAsNewUntil;

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // ✅ Save sessions
            if (course.IsMultiSession && request.Sessions != null && request.Sessions.Any())
            {
                var sessions = request.Sessions.Select(s => new CourseSession
                {
                    CourseSysId = course.CourseSysId,
                    SessionDate = s.SessionDate,
                    StartTime = TimeSpan.Parse(s.StartTime),
                    EndTime = TimeSpan.Parse(s.EndTime),
                    SessionUrl = s.SessionUrl,
                    TrainingLocation = s.TrainingLocation
                }).ToList();

                _context.CourseSessions.AddRange(sessions);
                await _context.SaveChangesAsync();
            }

            // ✅ Calculate BaseHours
            if (course.CourseTimeBegin.HasValue && course.CourseTimeEnd.HasValue)
            {
                var start = course.CourseTimeBegin.Value;
                var end = course.CourseTimeEnd.Value;

                double totalHours = (end - start).TotalHours;
                if (totalHours < 0) totalHours = 0;

                course.BaseHours = (decimal)Math.Round(totalHours, 2);
                await _context.SaveChangesAsync();
            }

            //  IMPORTANT: update SubjectTopics for this subject (no new endpoint)
            //  IMPORTANT: update SubjectTopics for this subject (same endpoint)
            if (course.SubjectSysId > 0 && request.TopicCodes != null && request.TopicCodes.Any())
            {
                var subjectId = course.SubjectSysId;   //  int, no .Value

                var topicCodes = request.TopicCodes
                    .Distinct()
                    .ToList();

                // delete old mappings for this subject
                var existing = await _context.SubjectTopics
                    .Where(x => x.SubjectSysId == subjectId)
                    .ToListAsync();

                _context.SubjectTopics.RemoveRange(existing);

                // insert new mappings
                foreach (var code in topicCodes)
                {
                    _context.SubjectTopics.Add(new SubjectTopic
                    {
                        SubjectSysId = subjectId,
                        TopicCode = code
                    });
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Course scheduled successfully!", courseId = course.CourseSysId });
        }

    }
}

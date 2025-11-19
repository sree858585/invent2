using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIVTraining_Vue.Server.Models
{
    public class CourseSession
    {
        public int Id { get; set; }
        public int CourseSysId { get; set; }   // FK to Course
        public DateTime SessionDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? SessionUrl { get; set; }

        [ForeignKey("CourseSysId")]
        public Course Course { get; set; }
        public string TrainingLocation { get; set; }
    }
}


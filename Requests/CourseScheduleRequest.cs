using System;
using HIVTraining_Vue.Server.Models;
using System.Collections.Generic;   //  add


namespace HIVTraining_Vue.Server.Requests
{
    public class CourseScheduleRequest
    {
        public Course Course { get; set; }
        public List<SessionRequest>? Sessions { get; set; }
        public List<int>? TopicCodes { get; set; }

    }

    public class SessionRequest
    {
        public DateTime SessionDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string? SessionUrl { get; set; }
        public string? TrainingLocation { get; set; }

    }
}


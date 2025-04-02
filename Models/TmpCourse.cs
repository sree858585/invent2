using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class TmpCourse
{
    public int CourseSysId { get; set; }

    public int SiteSysId { get; set; }

    public int SubjectSysId { get; set; }

    public DateTime? CourseDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? CourseTime { get; set; }

    public string? Information { get; set; }

    public int? Instructor1 { get; set; }

    public int? Instructor2 { get; set; }

    public bool? Delivered { get; set; }

    public bool? Cancelled { get; set; }

    public string? CancellReason { get; set; }

    public DateTime? DateEntered { get; set; }

    public DateTime? DateModified { get; set; }

    public string? City { get; set; }

    public string? TrainingLocation { get; set; }

    public DateTime? RegDeadLine { get; set; }

    public int? Deliverable { get; set; }

    public int? MaxSeats { get; set; }

    public int? Format { get; set; }

    public int? ContractType { get; set; }

    public int? Region { get; set; }

    public bool? Rtc { get; set; }

    public bool? Coe { get; set; }

    public bool? OtherFund { get; set; }

    public bool Hidden { get; set; }

    public bool InHseTraining { get; set; }
}

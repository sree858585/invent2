using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class TmpUserCourse
{
    public int UserCourseSysId { get; set; }

    public int UserSysId { get; set; }

    public int CourseSysId { get; set; }

    public DateTime? DateEntered { get; set; }

    public DateTime? DateModified { get; set; }

    public int? Status { get; set; }

    public DateTime? DateStatusChanged { get; set; }

    public string? CancelReason { get; set; }

    public bool? Attended { get; set; }

    public bool? EmailSend { get; set; }

    public Guid? Token { get; set; }

    public int? Score { get; set; }

    public int? Attempt { get; set; }

    public int? TrainingStatus { get; set; }
}

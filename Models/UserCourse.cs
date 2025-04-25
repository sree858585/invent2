using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class UserCourse
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

    /// <summary>
    /// Special accommodations under the Americans with Disability Act (ADA)
    /// </summary>
    public bool? Adaneed { get; set; }

    /// <summary>
    /// Special accommodations under the Americans with Disability Act (ADA)
    /// </summary>
    public string? Adadetails { get; set; }

    public int Hybrid { get; set; }

    public bool IsWaitlisted { get; set; } = false;
    public int? WaitlistNumber { get; set; }
}

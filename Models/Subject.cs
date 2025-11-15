using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class Subject
{
    public int SubjectSysId { get; set; }

    public string? CourseTitle { get; set; }

    public string? Description { get; set; }

    public int? Category { get; set; }

    public bool Ai { get; set; }

    public bool Active { get; set; }

    public string? ApprovedCode { get; set; }

    public string? CertDescription { get; set; }

    public string? MiscCertDesc { get; set; }

    public string? CreditHrs { get; set; }

    public bool Cnecredits { get; set; }

    public bool Oasascredits { get; set; }

    public bool PeerCertCredits { get; set; }

    public string? VideoUrl { get; set; }

    /// <summary>
    /// Indicate this is The Gaming Agency course or not
    /// </summary>
    public bool Is3rdParty { get; set; }

    public string? A3rdPartyCrseId { get; set; }

    public bool IsPeerCore { get; set; }
    public bool IsOnlineTraining { get; set; }
}

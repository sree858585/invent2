using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class Instructor
{
    public int InstructorSysId { get; set; }

    public int? SiteSysId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? CellPhone { get; set; }

    public bool? Active { get; set; }

    public string? InsNotes { get; set; }
}

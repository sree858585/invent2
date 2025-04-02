using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class VCourseListing
{
    public int SubjectSysId { get; set; }

    public string? CourseTitle { get; set; }

    public string? Description { get; set; }

    public string? ArrFormat { get; set; }

    public string? ArrRegion { get; set; }

    public string? ArrSite { get; set; }

    public bool Highlight { get; set; }

    public int SortKey { get; set; }

    public bool Active { get; set; }

    public int ListId { get; set; }

    public string? ArrCourseDate { get; set; }

    public string? ArrApproval { get; set; }
}

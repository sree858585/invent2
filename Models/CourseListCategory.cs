using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class CourseListCategory
{
    public int CourseListSysId { get; set; }

    public string? PageTitle { get; set; }

    public string? CategoryTitle { get; set; }

    public int SortKey { get; set; }

    public bool Active { get; set; }

    public DateTime? CreateDt { get; set; }
}

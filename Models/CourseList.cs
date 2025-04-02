using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class CourseList
{
    public int ListId { get; set; }

    public int CourseListSysId { get; set; }

    public int SortKey { get; set; }

    public string? Title { get; set; }

    public bool Active { get; set; }

    public DateTime? CreateDt { get; set; }

    public bool Highlight { get; set; }

    public int? SubjectSysId { get; set; }
}

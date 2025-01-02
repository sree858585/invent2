using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HIVTraining_Vue.Models;

public partial class CourseListing
{
    public int ListId { get; set; }

    public int SortKey { get; set; }

    public string? Title { get; set; }

    public bool Active { get; set; }

    public DateTime? CreateDt { get; set; }

    public bool Highlight { get; set; }

    public int SubjectSysId { get; set; }

    // Navigation property to Subject
    [ForeignKey("SubjectSysId")]
    public Subject? Subject { get; set; }
}

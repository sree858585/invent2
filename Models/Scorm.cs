using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIVTraining_Vue.Server.Models;
[Table("Scorm")]
public partial class Scorm
{
    public int Id { get; set; }
    public int? Course { get; set; }
    public string? Name { get; set; }
    public string? Scormtype { get; set; }
    public string? Reference { get; set; }
    public string? Version { get; set; }
    public int? Maxattempt { get; set; }
    public int? Forcecompleted { get; set; }
    public int? Forcenewattempt { get; set; }
    public string? Launch { get; set; }
    public DateTime? Timeopen { get; set; }
    public DateTime? Timeclose { get; set; }
    public DateTime? Timemodified { get; set; }
    public int? Autocommit { get; set; }
    // (you can add the rest later as needed)
}
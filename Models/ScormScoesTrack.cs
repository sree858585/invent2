using System;

namespace HIVTraining_Vue.Server.Models;

public partial class ScormScoesTrack
{
    public int Id { get; set; }

    public int? Userid { get; set; }
    public int? Scormid { get; set; }

    public int? Scoid { get; set; }
    public int? Attempt { get; set; }

    public string? Element { get; set; }
    public string? Value { get; set; }

    public int? Timemodified { get; set; }
}
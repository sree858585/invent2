namespace HIVTraining_Vue.Server.Models;

public partial class ScormAiccSession
{
    public int Id { get; set; }

    public int Userid { get; set; }         // NOT NULL in DB
    public int Scormid { get; set; }        // NOT NULL in DB

    public string Hacpsession { get; set; } = null!;  // NOT NULL in DB

    public int? Scoid { get; set; }         // ✅ DB says int NULL

    public string? Scormmode { get; set; }
    public string? Scormstatus { get; set; }
    public int? Attempt { get; set; }
    public string? Lessonstatus { get; set; }
    public string? Sessiontime { get; set; }

    public DateTime? Timecreated { get; set; }   // ✅ DB is datetime/datetime2
    public DateTime? Timemodified { get; set; }
}
using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class PeerAgency
{
    public int PeerAgencySysId { get; set; }

    public int PeerSysId { get; set; }

    public string? Agency { get; set; }

    public string? Address { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public DateTime CreateDate { get; set; }
}

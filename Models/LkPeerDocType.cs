using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class LkPeerDocType
{
    public int PeerDocId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool Mandatary { get; set; }

    public bool? Optional { get; set; }

    public int Rank { get; set; }

    public bool Active { get; set; }

    public string DocAbbrev { get; set; } = null!;
}

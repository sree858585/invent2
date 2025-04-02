using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class LkRegionCnty
{
    public int Code { get; set; }

    public int ParentId { get; set; }

    public string Value { get; set; } = null!;

    public int? SortKey { get; set; }
}

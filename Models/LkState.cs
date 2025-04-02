using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class LkState
{
    public string Code { get; set; } = null!;

    public string Value { get; set; } = null!;

    public int? SortKey { get; set; }
}

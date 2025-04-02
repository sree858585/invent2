using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class LkEthnicity
{
    public int Code { get; set; }

    public string Value { get; set; } = null!;

    public int? SortKey { get; set; }
}

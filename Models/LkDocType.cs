using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class LkDocType
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Value { get; set; } = null!;

    public int SortKey { get; set; }

    public bool Active { get; set; }
}

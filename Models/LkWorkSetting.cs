using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class LkWorkSetting
{
    public int Code { get; set; }

    public string Value { get; set; } = null!;

    public int? Sortkey { get; set; }

    public bool IsActive { get; set; }
}

using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class TempReset
{
    public int Id { get; set; }

    public string User { get; set; } = null!;

    public string Temp { get; set; } = null!;

    public DateTime Date { get; set; }
}

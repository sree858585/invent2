using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class AppErrorLog
{
    public int LogId { get; set; }

    public string? Message { get; set; }

    public DateTime? DateOccur { get; set; }
}

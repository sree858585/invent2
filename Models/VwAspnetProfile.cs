using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class VwAspnetProfile
{
    public Guid UserId { get; set; }

    public DateTime LastUpdatedDate { get; set; }

    public int? DataSize { get; set; }
}

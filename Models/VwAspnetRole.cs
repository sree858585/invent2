using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class VwAspnetRole
{
    public Guid ApplicationId { get; set; }

    public Guid RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string LoweredRoleName { get; set; } = null!;

    public string? Description { get; set; }
}

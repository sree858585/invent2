using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class AspnetUsersInRole
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public virtual AspnetRole Role { get; set; } = null!;
}

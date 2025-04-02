using System;
using System.Collections.Generic;

namespace HIVTraining_Vue.Server.Models;

public partial class VwMembershipUser
{
    public string? Username { get; set; }

    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }
}

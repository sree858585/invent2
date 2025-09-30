using System;
using Microsoft.AspNetCore.Identity;

namespace HIVTraining_Vue.Server.Models
{
	public class ApplicationUser : IdentityUser
	{
        public bool IsUsingOldPassword { get; set; }

    }
}


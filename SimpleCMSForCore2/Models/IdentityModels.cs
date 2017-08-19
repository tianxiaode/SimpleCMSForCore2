using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace SimpleCMSForCore2.Models
{

    public class ApplicationRole : IdentityRole<int>
    {
        public string Description { get; set; }
    }

    public class ApplicationUser : IdentityUser<int>
    {
        public bool IsApprove { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastLogin { get; set; }

        public virtual  ICollection<UserProfile> UserProfiles { get; set; }
    }

}
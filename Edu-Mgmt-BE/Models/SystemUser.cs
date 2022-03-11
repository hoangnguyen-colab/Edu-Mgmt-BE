using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class SystemUser
    {
        public SystemUser()
        {
            UserDetail = new HashSet<UserDetail>();
        }

        public Guid SystemUserId { get; set; }
        public string Username { get; set; }
        public string UserUsername { get; set; }
        public string UserPassword { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public DateTime? CreatedUser { get; set; }
        public DateTime? ModifyUser { get; set; }

        public virtual ICollection<UserDetail> UserDetail { get; set; }
    }
}

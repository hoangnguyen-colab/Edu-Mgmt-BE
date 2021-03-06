using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class UserDetail
    {
        public Guid UserDetailId { get; set; }
        public Guid? UserId { get; set; }
        public Guid SystemUserId { get; set; }
        public int SystemRoleId { get; set; }
    }
}

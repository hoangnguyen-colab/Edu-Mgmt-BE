using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class SystemUser
    {
        public Guid SystemUserId { get; set; }
        public string UserUsername { get; set; }
        public string UserPassword { get; set; }
        public string Fullname { get; set; }
        public string UserPhone { get; set; }
        public string UserDob { get; set; }
        public string UserGender { get; set; }
    }
}

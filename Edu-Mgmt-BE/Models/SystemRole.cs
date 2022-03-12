﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class SystemRole
    {
        public SystemRole()
        {
            UserDetail = new HashSet<UserDetail>();
        }

        public int RoleId { get; set; }
        public string RoleName { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<UserDetail> UserDetail { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class Class
    {
        public Class()
        {
            ClassDetail = new HashSet<ClassDetail>();
        }

        public Guid ClassId { get; set; }
        public string ClassName { get; set; }
        public string ClassYear { get; set; }
        public Guid TeacherId { get; set; }

        public virtual Teacher Teacher { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<ClassDetail> ClassDetail { get; set; }
    }
}

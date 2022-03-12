﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class SchoolYear
    {
        public SchoolYear()
        {
            ClassDetail = new HashSet<ClassDetail>();
        }

        public Guid SchoolYearId { get; set; }
        public string SchoolYearName { get; set; }
        public string SchoolYearDate { get; set; }
        public string ActiveYear { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public DateTime? CreatedUser { get; set; }
        public DateTime? ModifyUser { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<ClassDetail> ClassDetail { get; set; }
    }
}
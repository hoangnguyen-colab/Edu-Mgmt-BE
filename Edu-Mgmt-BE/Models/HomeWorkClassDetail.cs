using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class HomeWorkClassDetail
    {
        public Guid HomeWorkClassDetail1 { get; set; }
        public Guid? HomeWorkId { get; set; }
        public Guid ClassId { get; set; }

        [IgnoreDataMember]
        [JsonIgnore]
        public virtual Class Class { get; set; }
        [IgnoreDataMember]
        [JsonIgnore]
        public virtual HomeWork HomeWork { get; set; }
    }
}

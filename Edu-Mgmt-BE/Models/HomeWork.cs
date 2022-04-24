using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class HomeWork
    {
        public HomeWork()
        {
            Answer = new HashSet<Answer>();
            HomeWorkClassDetail = new HashSet<HomeWorkClassDetail>();
            HomeWorkFileDetail = new HashSet<HomeWorkFileDetail>();
        }

        public Guid HomeWorkId { get; set; }
        public string HomeWorkName { get; set; }
        public string HomeWorkType { get; set; }
        public string HomeWorkContent { get; set; }
        public DateTime? DueDate { get; set; }
        public int? HomeWorkStatus { get; set; }
        public bool? RequiredLogin { get; set; }
        public bool? OnlyAssignStudent { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? TeacherId { get; set; }

        [IgnoreDataMember]
        [JsonIgnore]
        public virtual ICollection<Answer> Answer { get; set; }
        [IgnoreDataMember]
        [JsonIgnore]
        public virtual ICollection<HomeWorkClassDetail> HomeWorkClassDetail { get; set; }
        [IgnoreDataMember]
        [JsonIgnore]
        public virtual ICollection<HomeWorkFileDetail> HomeWorkFileDetail { get; set; }
    }
}

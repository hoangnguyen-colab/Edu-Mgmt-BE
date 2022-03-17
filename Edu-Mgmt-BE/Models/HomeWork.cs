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
            HomeWorkFileDetail = new HashSet<HomeWorkFileDetail>();
            HomeWorkResultDetail = new HashSet<HomeWorkResultDetail>();
        }

        public Guid HomeWorkId { get; set; }
        public string HomeWorkName { get; set; }
        public string HomeWorkType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? TeacherId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<HomeWorkFileDetail> HomeWorkFileDetail { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<HomeWorkResultDetail> HomeWorkResultDetail { get; set; }
    }
}

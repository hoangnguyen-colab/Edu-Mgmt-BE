using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class Result
    {
        public Result()
        {
            HomeWorkResultDetail = new HashSet<HomeWorkResultDetail>();
        }

        public Guid ResultId { get; set; }
        public string FinalScore { get; set; }
        public DateTime? CreatedDate { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<HomeWorkResultDetail> HomeWorkResultDetail { get; set; }
    }
}

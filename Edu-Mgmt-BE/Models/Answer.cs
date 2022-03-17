using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class Answer
    {
        public Answer()
        {
            AnswerFileDetail = new HashSet<AnswerFileDetail>();
            HomeWorkResultDetail = new HashSet<HomeWorkResultDetail>();
        }

        public Guid AnswerId { get; set; }
        public string AnswerName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string FinishDuration { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<AnswerFileDetail> AnswerFileDetail { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<HomeWorkResultDetail> HomeWorkResultDetail { get; set; }
    }
}

using System;
using System.Collections.Generic;

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
        public DateTime StartTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public string FinishDuration { get; set; }

        public virtual ICollection<AnswerFileDetail> AnswerFileDetail { get; set; }
        public virtual ICollection<HomeWorkResultDetail> HomeWorkResultDetail { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class FileUpload
    {
        public FileUpload()
        {
            AnswerFileDetail = new HashSet<AnswerFileDetail>();
            HomeWorkFileDetail = new HashSet<HomeWorkFileDetail>();
        }

        public Guid FileUploadId { get; set; }
        public string FileUploadUrl { get; set; }
        public string FileUploadName { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<AnswerFileDetail> AnswerFileDetail { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<HomeWorkFileDetail> HomeWorkFileDetail { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Edu_Mgmt_BE.Models
{
    public partial class HomeWorkFileDetail
    {
        public Guid FileUploadDetailId { get; set; }
        public Guid? FileUploadId { get; set; }
        public Guid HomeWorkId { get; set; }

        [IgnoreDataMember]
        [JsonIgnore]
        public virtual FileUpload FileUpload { get; set; }
        [IgnoreDataMember]
        [JsonIgnore]
        public virtual HomeWork HomeWork { get; set; }
    }
}

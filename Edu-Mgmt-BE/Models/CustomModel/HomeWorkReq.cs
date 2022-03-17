using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Models.CustomModel
{
    public class HomeWorkReq
    {
        public string HomeWorkName { get; set; }
        public string HomeWorkType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public double DueDate { get; set; }
        public FileUpload[] FileList { get; set; }
    }
}

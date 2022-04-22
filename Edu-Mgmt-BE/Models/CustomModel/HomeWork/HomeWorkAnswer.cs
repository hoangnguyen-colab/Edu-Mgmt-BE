using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Models.CustomModel.HomeWork
{
    public class HomeWorkAnswer
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentDob { get; set; }
        public string StudentPhone { get; set; }
        public DateTime SubmitTime { get; set; }
        public string AnswerContent { get; set; }
        public FileUpload[] FileList { get; set; }
        public Guid ClassId { get; set; }
        public Guid HomeWorkId { get; set; }
    }
}

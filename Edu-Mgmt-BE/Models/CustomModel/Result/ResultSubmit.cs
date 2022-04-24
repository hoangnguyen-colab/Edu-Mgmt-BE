using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Models.CustomModel.Result
{
    public class ResultSubmit
    {
        public string FinalScore { get; set; }
        public string ResultContent { get; set; }
        public Guid AnswerId { get; set; }
    }
}

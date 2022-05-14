using System;
namespace Edu_Mgmt_BE.Models.CustomModel.Result
{
    public class ResultReviewRequest
    {
        public ResultReviewRequest()
        {

        }
        public string ReviewContent { get; set; }
        public Guid ResultId { get; set; }
    }
}

using System;
namespace Edu_Mgmt_BE.Models.CustomModel.Student
{
    public class StudentAnswer
    {
        public StudentAnswer()
        {
        }

        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentPhone { get; set; }
        public string StudentDob { get; set; }
        public string StudentGender { get; set; }
        public Guid AnswerId { get; set; }
        public string FinalScore { get; set; }
    }
}

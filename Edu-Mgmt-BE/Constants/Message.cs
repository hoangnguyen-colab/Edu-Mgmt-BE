using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Constants
{
    public class Message
    {
        //General
        public const string SuccessMsg = "Thành công.";
        public const string ErrorMsg = "Có lỗi xảy ra.";
        public const string TitleError = "Tiêu đề không thể trống.";
        public const string NotFound = "Không tìm thấy.";
        public const string BadRequest = "Không hợp lệ.";

        //Xử lý message tài khoản
        public const string NotAuthorize = "Bạn không có quyền này.";
        public const string LoginIncorrect = "Thông tin đăng nhập không chính xác!.";
        public const string AccountNotFound = "Không tìm thấy tài khoản này.";
        public const string AccountUsernameExist = "Username đã tồn tại.";
        public const string InvalidDOB = "Ngày sinh không hợp lệ";
        public const string InvalidPhone = "Số điện thoại không hợp lệ";

        public const string AccountLoginAgain = "Bạn vui lòng đăng nhập lại để thực hiện chức năng này.";
        public const string AccountLogoutSuccess = "Đăng xuất thành công.";
        public const string AccountLogLogin = "Login.";
        public const string AccountLogDelete = "Xóa tài khoản.";
        public const string AccountLogLogout = "Logout.";
        public const string AccountLogChange = "Thay đổi thông tin tài khoản.";
        public const string AccountLogPassword = "Thay đổi mật khẩu.";

        //Xử lý message user account
        public const string UserNameEmpty = "Tên người dùng không thể trống.";
        public const string UserLoginNameEmpty = "Tên đăng nhập không thể trống.";
        public const string UserPasswordEmpty = "Mật khẩu không thể trống.";
        public const string UserLoginNameExist = "Tên đăng nhập đã tồn tại.";
        public const string UserAccountExist = "Tài khoản đã tồn tại.";

        //Xử lý message file
        public const string FileUploadFail = "Tải lên file thất bại.";
        public const string FileEmpty = "File trống.";
        public const string FileNotFound = "Không tìm thấy file.";
        public const string FileReadFail = "Đọc file thất bại.";
        public const string FileServerFail = "Đã có lỗi, thử lại sau hoặc sử dụng định dạng file theo mẫu.";

        //Xử lý message student
        public const string StudentNotFound = "Không tìm thấy học sinh này.";
        public const string StudentExist = "Học sinh đã tồn tại.";
        public const string StudentNameEmpty = "Tên học sinh không thể trống.";
        public const string StudentEmpty = "Học sinh không thể trống.";
        public const string StudentPhoneEmpty = "Số điện thoại của học sinh không thể trống.";

        //Xử lý message lớp
        public const string ClassNotFound = "Không tìm thấy lớp này.";
        public const string ClassExist = "Lớp đã tồn tại.";
        public const string ClassNameEmpty = "Tên lớp không thể trống.";
        public const string ClassIdEmpty = "Mã lớp không thể trống.";
        public const string ClassIdExist = "Mã lớp đã tồn tại.";
        public const string ClassEmpty = "Lớp không thể trống.";
        public const string ClassSubmit = "Hãy chọn ít nhất một lớp học để đăng ký.";

        //Xử lý message bài tập
        public const string HomeWorkNotFound = "Không tìm thấy bài tập này.";
        public const string HomeWorkNameEmpty = "Tên bài tập không thể trống.";
        public const string HomeWorkTypeEmpty = "Loại bài tập không thể trống.";
        public const string HomeWorkClassEmpty = "Lớp giao bài tập không thể trống.";
        public const string HomeWorkEmpty = "Bài tập không thể trống.";
        public const string HomeWorkOnlyAssignStudent = "Chỉ học sinh trong danh sách làm bài tập này. Vui lòng liên hệ giáo viên hướng dẫn";
        public const string HomeDueDate = "Bài tập đã quá hạn nộp.";
        public const string HomeWorkStatusExpired = "Bài tập đã kết thúc.";

        //Xử lý message bài nộp
        public const string AnswerEmpty = "Bài nộp của học sinh trống.";
        public const string AnswerNotFound = "Không tìm thấy bài nộp này.";
        public const string AnswerAlreadySubmit = "Học sinh đã nộp bài. Không thể nộp lại";

        //Xử lý message phúc khảo
        public const string ReviewExisted = "Bài làm này đã gửi phúc khảo.";

        //Xử lý message đáp án
        public const string ResultNotFound = "Không tìm thấy bài nộp này.";
        public const string ResultSubmit = "Học sinh đã nộp bài.";
        public const string ResultScoreBadReq = "Điểm không hợp lệ.";
        public const string ResultAlreadySubmit = "Bài đã được chấm điểm.";

        //Xử lý message giáo viên
        public const string TeacherNotFound = "Không tìm thấy giáo viên này.";
        public const string TeacherExist = "Giáo viên đã tồn tại.";
        public const string TeacherNameEmpty = "Tên giáo viên không thể trống.";
        public const string TeacherPhoneEmpty = "Số điện thoại giáo viên không thể trống.";
        public const string TeacherPhoneExist = "Số điện thoại đã tồn tại.";
        public const string TeacherIdEmpty = "Mã giáo viên không thể trống.";
        public const string TeacherIdBadRequest = "Mã giáo viên không hợp lệ.";
        public const string TeacherIdExist = "Mã giáo viên đã tồn tại.";

        //Xử lý message School Year
        public const string SchoolYearNameEmpty = "Tên năm học không thể trống.";
        public const string SchoolYearDateEmpty = "Năm học không thể trống.";
        public const string SchoolYearExist = "Năm học đã tồn tại.";
        public const string SchoolYearNotFound = "Không tìm năm học này.";
        public const string SchoolYearBadRequest = "Năm học không hợp lệ.";
    }
}

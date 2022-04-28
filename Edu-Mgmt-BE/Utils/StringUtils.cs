using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Edu_Mgmt_BE.Utils
{
    public class StringUtils
    {
        private static readonly string[] VietnameseSigns = new string[]
           {

            "aAeEoOuUiIdDyY",

            "áàạảãâấầậẩẫăắằặẳẵ",

            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",

            "éèẹẻẽêếềệểễ",

            "ÉÈẸẺẼÊẾỀỆỂỄ",

            "óòọỏõôốồộổỗơớờợởỡ",

            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",

            "úùụủũưứừựửữ",

            "ÚÙỤỦŨƯỨỪỰỬỮ",

            "íìịỉĩ",

            "ÍÌỊỈĨ",

            "đ",

            "Đ",

            "ýỳỵỷỹ",

            "ÝỲỴỶỸ"
           };
        
        public static string VietnameseNormalize(string str)
        {
            string formatString = str.Trim().ToLower();
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    formatString = formatString.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return formatString;
        }

        public static bool IsPhoneNumber(string number)
        {
            return Regex.Match(number, @"^([\+]?84[-]?|[0])?[1-9][0-9]{8}$").Success;
        }
    }
}

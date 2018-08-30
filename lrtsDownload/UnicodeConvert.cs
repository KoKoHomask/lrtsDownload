using System;
using System.Text.RegularExpressions;

namespace lrtsDownload
{
    public class UnicodeConvert
    {
        /// <summary>
        /// unicode转中文（符合js规则的）
        /// </summary>
        /// <returns></returns>
        public static string unicode_js_1(string str)
        {
            String ss = "";
            //List<String> list = new List<string>();
            int num = str.Length / 4;
            for (int i = 0; i < num; i++)
            {
                ss = ss + "\\u" + str.Substring(i * 4, 4);
            }
            str = ss;
            string outStr = "";
            Regex reg = new Regex(@"(?i)\\u([0-9a-f]{4})");
            outStr = reg.Replace(str, delegate (Match m1)
            {
                return ((char)Convert.ToInt32(m1.Groups[1].Value, 16)).ToString();
            });
            return outStr + "//" + ss;
        }
        /// 中英文转unicode
        /// </summary>
        /// <returns></returns>
        public string unicode_0(string str)
        {
            string outStr = "";
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    String ss = ((int)str[i]).ToString("x");
                    if (ss.Length != 4)
                    {
                        for (int jj = 0; jj <= 4 - ss.Length; jj++)
                        {
                            ss = "0" + ss;
                        }

                    }
                    outStr += "\\u" + ss;
                }
            }
            outStr = outStr.Replace("\\u", "");
            return outStr;
        }
    }
}

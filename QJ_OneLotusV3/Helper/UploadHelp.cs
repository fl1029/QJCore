using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QJ_OneLotusV3
{
    public class UploadHelp
    {

        public static string GetFileType(string extname)
        {
            switch (extname.ToLower())
            {
                case "gif":
                case "jpg":
                case "bmp":
                case "jpeg":
                case "tiff":
                case "png":
                    return "pic";
                default:
                    return extname.ToLower().TrimStart('.');
            }
        }


        /// <summary>
        /// 生成指定长度的随机码。
        /// </summary>
        public static string CreateRandomCode(int length)
        {
            string[] codes = new string[36] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            StringBuilder randomCode = new StringBuilder();
            Random rand = new Random();
            for (int i = 0; i < length; i++)
            {
                randomCode.Append(codes[rand.Next(codes.Length)]);
            }
            return randomCode.ToString();
        }

        /// <summary>
        /// 表示图片的上传结果。
        /// </summary>
      
    }


}

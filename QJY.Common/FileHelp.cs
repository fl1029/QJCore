using QJY.Data;
using System;
using System.Collections.Generic;
using System.Net;

namespace QJY.Common
{
    public class FileHelp
    {



        /// <summary>
        /// 压缩需求
        /// </summary>
        /// <param name="strFileData"></param>
        /// <param name="UserInfo"></param>
        /// <returns></returns>
        public string CompressZip(string strFileData, JH_Auth_QY QYinfo)
        {
            Dictionary<String, String> DATA = new Dictionary<String, String>();
            DATA.Add("data", strFileData);
            HttpWebResponse ResponseData = CommonHelp.CreatePostHttpResponse(QYinfo.FileServerUrl.TrimEnd('/') + "/" +QYinfo.QYCode + "/document/zipfolder", DATA, 0, "", null);
            return CommonHelp.GetResponseString(ResponseData);
        }


    }
}

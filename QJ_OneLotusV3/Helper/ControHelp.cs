using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QJY.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QJ_OneLotusV3
{
    public class ControHelp
    {
        public static string CovJson(Msg_Result Model)
        {
            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string Result = JsonConvert.SerializeObject(Model, Formatting.Indented, timeConverter).Replace("null", "\"\"");
            return Result;
        }
    }
    public class NPOIMemoryStream : MemoryStream
    {
        /// <summary>
        /// 获取流是否关闭
        /// </summary>
        public bool IsColse
        {
            get;
            private set;
        }

        public NPOIMemoryStream(bool colse = false)
        {
            IsColse = colse;
        }

        public override void Close()
        {
            if (IsColse)
            {
                base.Close();
            }

        }
    }
}

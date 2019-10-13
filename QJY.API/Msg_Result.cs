using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QJY.API
{
    /// <summary>
    /// 返回消息类
    /// </summary>
    public class Msg_Result
    {
        [JsonProperty("Action")]
        public string Action { get; set; }

        [JsonProperty("ErrorMsg")]
        public string ErrorMsg { get; set; }
        public int DataLength { get; set; }
        public string ResultType { get; set; }

        [JsonProperty("Result")]
        public dynamic Result { get; set; }


        [JsonProperty("Result1")]
        public dynamic Result1 { get; set; }


        [JsonProperty("Result2")]
        public dynamic Result2 { get; set; }


        [JsonProperty("Result3")]
        public dynamic Result3 { get; set; }

        [JsonProperty("Result4")]
        public dynamic Result4 { get; set; }


        [JsonProperty("Result5")]
        public dynamic Result5 { get; set; }


        [JsonProperty("Result6")]
        public dynamic Result6 { get; set; }

        [JsonProperty("uptoken")]
        public dynamic uptoken { get; set; }
    }
}

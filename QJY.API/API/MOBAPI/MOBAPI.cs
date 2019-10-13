using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QJY.Data;
using QJY.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using QJY.Common;

namespace QJY.API
{
    public class MOBAPI 
    {



      
        #region 验证企业号签名
        /// <summary>
        /// 验证企业号签名
        /// </summary>
        /// <param name="token">企业号配置的Token</param>
        /// <param name="signature">签名内容</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="nonce">nonce参数</param>
        /// <param name="corpId">企业号ID标识</param>
        /// <param name="encodingAESKey">加密键</param>
        /// <param name="echostr">内容字符串</param>
        /// <param name="retEchostr">返回的字符串</param>
        /// <returns></returns>
        public bool CheckSignature(string token, string signature, string timestamp, string nonce, string corpId, string encodingAESKey, string echostr, ref string retEchostr)
        {

            int result = new WXHelp().CheckSignature(token, encodingAESKey, corpId, signature, timestamp, nonce, echostr, ref retEchostr);
            if (result != 0)
            {
                CommonHelp.WriteLOG("ERR: VerifyURL fail, ret: " + result);
                return false;
            }

            return true;

            //ret==0表示验证成功，retEchostr参数表示明文，用户需要将retEchostr作为get请求的返回参数，返回给企业号。
            // HttpUtils.SetResponse(retEchostr);
        }

        #endregion


        public void GETSIGNAGURE( Msg_Result msg, string P1, string P2, JH_Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                WXHelp wx = new WXHelp(UserInfo.QYinfo);
                string url = P1;
                string jsapi_ticket = wx.GetTicket().ticket;
                string noncestr = CreatenNonce_str();
                long timestamp = CreatenTimestamp();

                var string1Builder = new StringBuilder();
                string1Builder.Append("jsapi_ticket=").Append(jsapi_ticket).Append("&")
                              .Append("noncestr=").Append(noncestr).Append("&")
                              .Append("timestamp=").Append(timestamp).Append("&")
                              .Append("url=").Append(url.IndexOf("#") >= 0 ? url.Substring(0, url.IndexOf("#")) : url);
                string string1 = string1Builder.ToString();

                byte[] StrRes = Encoding.Default.GetBytes(string1);
                HashAlgorithm iSHA = new SHA1CryptoServiceProvider();
                StrRes = iSHA.ComputeHash(StrRes);
                StringBuilder EnText = new StringBuilder();
                foreach (byte iByte in StrRes)
                {
                    EnText.AppendFormat("{0:x2}", iByte);
                }
                //return EnText.ToString();

                msg.Result =
                    new JObject(
                        new JProperty("appId", UserInfo.QYinfo.corpId),
                        new JProperty("noncestr", noncestr),
                        new JProperty("timestamp", timestamp),
                        new JProperty("signature", EnText.ToString())
                        );

                //通讯录权限验证
                if (!string.IsNullOrEmpty(P2) && P2 == "GROUP")
                {
                    var g_ticket = wx.GetGroup_Ticket();
                    string noncestr2 = CreatenNonce_str();
                    long timestamp2 = CreatenTimestamp();

                    var string1Builder2 = new StringBuilder();
                    string1Builder2.Append("group_ticket=").Append(g_ticket.ticket).Append("&")
                                  .Append("noncestr=").Append(noncestr2).Append("&")
                                  .Append("timestamp=").Append(timestamp2).Append("&")
                                  .Append("url=").Append(url.IndexOf("#") >= 0 ? url.Substring(0, url.IndexOf("#")) : url);
                    string string2 = string1Builder2.ToString();

                    byte[] StrRes2 = Encoding.Default.GetBytes(string2);
                    HashAlgorithm iSHA2 = new SHA1CryptoServiceProvider();
                    StrRes2 = iSHA2.ComputeHash(StrRes2);
                    StringBuilder EnText2 = new StringBuilder();
                    foreach (byte iByte in StrRes2)
                    {
                        EnText2.AppendFormat("{0:x2}", iByte);
                    }

                    msg.Result1 =
                        new JObject(
                            new JProperty("group_id", g_ticket.group_id),
                            new JProperty("noncestr", noncestr2),
                            new JProperty("timestamp", timestamp2),
                            new JProperty("signature", EnText2.ToString())
                            );
                }
            }
            catch (Exception ex)
            {
                msg.ErrorMsg = ex.ToString();
            }

        }

        /// <summary>
        /// 生成随即字符串
        /// </summary>
        /// <returns></returns>
        public string CreatenNonce_str()
        {
            string[] strs = new string[]
                                 {
                                  "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z",
                                  "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"
                                 };
            Random r = new Random();
            var sb = new StringBuilder();
            var length = strs.Length;
            for (int i = 0; i < 15; i++)
            {
                sb.Append(strs[r.Next(length - 1)]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 创建时间戳
        /// </summary>
        /// <returns></returns>
        public long CreatenTimestamp()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

    }
}
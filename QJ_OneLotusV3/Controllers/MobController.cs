using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using QJY.API;
using QJY.Common;
using QJY.Data;
using System;
using System.Data;
using System.IO;
using System.Linq;

namespace QJ_OneLotusV3.Controllers
{


    /// <summary>
    /// 移动端接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MobController : ControllerBase
    {
        private IHttpContextAccessor _accessor;
        private IHostingEnvironment hostingEnv { get; set; }
        private IContentTypeProvider contentTypeProvider { get; set; }


        Msg_Result Model = new Msg_Result() { Action = "", ErrorMsg = "" };


        public MobController(IHttpContextAccessor accessor, IHostingEnvironment env)
        {
            _accessor = accessor;
            this.hostingEnv = env;

        }


        /// <summary>
        /// 执行业务接口
        /// </summary>
        /// <param name="Action"></param>
        /// <param name="PostData"></param>
        /// <returns></returns>
        [HttpPost]
        [HttpGet]

        public ActionResult<string> XXJS()
        {
            var context = _accessor.HttpContext;

            String strCorpID = context.Request.Query["corpid"].ToString();
            string strCode = context.Request.Query["Code"].ToString();
            try
            {
                JH_Auth_QY jaq = new JH_Auth_QYB().GetALLEntities().FirstOrDefault();
                JH_Auth_Model jam = new JH_Auth_ModelB().GetEntity(p => p.ModelCode == strCode);
                if (jaq != null && jam != null)
                {

                    string echoString = context.Request.Query["echoStr"].ToString();
                    string signature = context.Request.Query["msg_signature"].ToString();//企业号的 msg_signature
                    string timestamp = context.Request.Query["timestamp"].ToString();
                    string nonce = context.Request.Query["nonce"].ToString();

                    string decryptEchoString = "";
                    if (new MOBAPI().CheckSignature(jam.Token, signature, timestamp, nonce, jaq.corpId, jam.EncodingAESKey, echoString, ref decryptEchoString))
                    {
                        if (!string.IsNullOrEmpty(decryptEchoString))
                        {
                            Int64 v = Convert.ToInt64(decryptEchoString);
                            context.Response.Clear();

                            using (StreamWriter writer = new StreamWriter(Response.Body))
                            {
                                writer.Write(v);
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Model.ErrorMsg = ex.ToString();
                CommonHelp.WriteLOG(ex.ToString());
            }

            return ControHelp.CovJson(Model);
        }


        /// <summary>
        /// 获取企业微信用户信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [HttpGet]
        public ActionResult<string> GetUserCodeByCode()
        {

            try
            {
                var context = _accessor.HttpContext;

                #region 获取Code
                Model.ErrorMsg = "获取Code错误，请重试";

                string strCode = context.Request.Query["code"].ToString();
                string strCorpID = context.Request.Query["corpid"].ToString();
                string strModelCode = context.Request.Query["funcode"].ToString();

                if (!string.IsNullOrEmpty(strCode))
                {

                    var qy = new JH_Auth_QYB().GetEntity(p => p.corpId == strCorpID);
                    if (qy != null)
                    {
                        try
                        {
                            //通过微信接口获取用户名
                            WXHelp wx = new WXHelp(qy);
                            string username = wx.GetUserDataByCode(strCode, strModelCode);
                            if (!string.IsNullOrEmpty(username))
                            {
                                var jau = new JH_Auth_UserB().GetUserByUserName(qy.ComId, username);

                                if (jau != null)
                                {
                                    Model.ErrorMsg = "";
                                    Model.Result = JwtHelper.CreateJWT(username, "Admin");
                                    Model.Result1 = jau.UserName;
                                    Model.Result3 = qy.FileServerUrl;
                                }

                            }
                            else
                            {
                                Model.ErrorMsg = "当前用户不存在";
                            }
                        }
                        catch (Exception ex)
                        {
                            Model.ErrorMsg = ex.ToString();
                        }
                    }
                    else
                    {
                        Model.ErrorMsg = "当前企业号未在电脑端注册";
                    }

                }
                else
                {
                    Model.ErrorMsg = "Code为空";
                }
                #endregion
            }
            catch (Exception ex)
            {
                Model.ErrorMsg = "接口调用失败,请检查日志";
                Model.Result = ex.ToString();
                CommonHelp.WriteLOG(ex.ToString());
            }
            return ControHelp.CovJson(Model);
        }



        /// <summary>
        /// 初始化企业微信信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [HttpGet]
        public ActionResult<string> WXINIT()
        {
            try
            {
                var context = _accessor.HttpContext;
                #region 获取Code
                Model.ErrorMsg = "";
                string P1 = context.Request.Query["P1"].ToString();
                string P2 = context.Request.Query["P2"].ToString();
                string szhlcode = context.Request.Query["szhlcode"].ToString();
                TokenModelJWT tokenModel = JwtHelper.SerializeJWT(szhlcode);
                if (tokenModel.UserName == null)
                {
                    Model.ErrorMsg = "NOCODE";
                }
                else
                {
                    JH_Auth_UserB.UserInfo UserInfo = new JH_Auth_UserB().GetUserInfo(10334, tokenModel.UserName);
                    DataTable dtUsers = new JH_Auth_UserB().GetDTByCommand(" SELECT UserName,UserRealName,mobphone FROM JH_Auth_User where ComId='" + UserInfo.User.ComId + "'");
                    //获取选择用户需要的HTML和转化用户名需要的json数据
                    Model.Result = dtUsers;
                    JH_Auth_Common url = new JH_Auth_CommonB().GetEntity(p => p.ModelCode == P1 && p.MenuCode == P2);
                    if (url != null)
                    {
                        Model.Result1 = url.Url1;
                    }
                    Model.Result2 = UserInfo.User.UserName + "," + UserInfo.User.UserRealName + "," + UserInfo.User.BranchCode + "," + UserInfo.BranchInfo.DeptName;
                    Model.Result3 = UserInfo.QYinfo.FileServerUrl;
                    Model.Result4 = UserInfo.QYinfo.QYCode;
                }
             

                #endregion
            }
            catch (Exception ex)
            {
                Model.ErrorMsg = "WXINIT接口调用失败,请检查日志";
                Model.Result = ex.ToString();
                CommonHelp.WriteLOG(ex.ToString());
            }
            return ControHelp.CovJson(Model);
        }


        /// <summary>
        /// 提醒接口,每5秒执行一次
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [HttpGet]
        public void AUTOALERT()
        {
            try
            {
                TXSXAPI.AUTOALERT();
            }
            catch (Exception ex)
            {
                Model.ErrorMsg = "AUTOALERT接口调用失败,请检查日志";
                Model.Result = ex.ToString();
            }
        }



        /// <summary>
        /// 放弃
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [HttpGet]
        public ActionResult<string> ISEXIST()
        {
            try
            {
                var context = _accessor.HttpContext;
                Model.ErrorMsg = "获取Code错误，请重试";
                string szhlcode = context.Request.Query["szhlcode"].ToString();
                TokenModelJWT tokenModel = JwtHelper.SerializeJWT(szhlcode);
                if (tokenModel.UserName == null)
                {
                    Model.Result = "NOCODE";
                }
            }
            catch (Exception ex)
            {
                Model.ErrorMsg = "ISEXIST接口调用失败,请检查日志";
                Model.Result = ex.ToString();
                CommonHelp.WriteLOG(ex.ToString());
            }
            return ControHelp.CovJson(Model);
        }

    }
}
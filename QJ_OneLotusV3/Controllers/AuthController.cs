using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using QJY.API;
using QJY.Common;
using QJY.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace QJ_OneLotusV3.Controllers
{
    /// <summary>
    /// 系统基础框架接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IHttpContextAccessor _accessor;
        private IHostingEnvironment environment { get; set; }
        private IContentTypeProvider contentTypeProvider { get; set; }


        Msg_Result Model = new Msg_Result() { Action = "", ErrorMsg = "" };


        public AuthController(IHttpContextAccessor accessor)
        {
            _accessor = accessor;

        }

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name="PostData">上传数据</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<string> Login(Object PostData)
        {

            JObject JsonData = JObject.FromObject(PostData);
            string username = JsonData["UserName"] == null ? "" : JsonData["UserName"].ToString();
            string password = JsonData["password"] == null ? "" : JsonData["password"].ToString();
            Dictionary<string, string> results3 = JsonConvert.DeserializeObject<Dictionary<string, string>>(PostData.ToString());
            Model.ErrorMsg = "";
            JH_Auth_QY qyModel = new JH_Auth_QYB().GetALLEntities().First();
            password = CommonHelp.GetMD5(password);
            JH_Auth_User userInfo = new JH_Auth_User();
            List<JH_Auth_User> userList = new JH_Auth_UserB().GetEntities(d => (d.UserName == username || d.mobphone == username) && d.UserPass == password).ToList();
            if (userList.Count() == 0)
            {
                Model.ErrorMsg = "用户名或密码不正确";
            }
            else
            {
                userInfo = userList[0];
                if (userInfo.IsUse != "Y")
                {
                    Model.ErrorMsg = "用户被禁用,请联系管理员";
                }
                if (Model.ErrorMsg == "")
                {
                    Model.Result = JwtHelper.CreateJWT(username, "Admin");
                    Model.Result1 = userInfo.UserName;
                    Model.Result2 = qyModel.FileServerUrl;
                    Model.Result4 = userInfo;

                    CacheHelp.Remove(userInfo.UserName);

                }

            }


            return ControHelp.CovJson(Model); ;
        }


        /// <summary>
        /// 执行系统基础框架接口
        /// </summary>
        /// <param name="Action">操作Action</param>
        /// <param name="PostData">参数Json数据</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public ActionResult<string> ExeAction(string Action, Object PostData)
        {
            Model.Action = Action;
            var context = _accessor.HttpContext;
            var tokenHeader = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            TokenModelJWT tokenModel = JwtHelper.SerializeJWT(tokenHeader);

            if (new DateTimeOffset(DateTime.Now.AddMinutes(5)).ToUnixTimeSeconds() > tokenModel.Exp)
            {
                //需要更新Token
                Model.uptoken = JwtHelper.CreateJWT(tokenModel.UserName, "Admin");
            }
            JH_Auth_UserB.UserInfo UserInfo = CacheHelp.Get(tokenModel.UserName) as JH_Auth_UserB.UserInfo;
            if (UserInfo == null)
            {
                UserInfo = new JH_Auth_UserB().GetUserInfo(10334, tokenModel.UserName);
                CacheHelp.Set(tokenModel.UserName, UserInfo);
            }
            try
            {

                JObject JsonData = JObject.FromObject(PostData);
                string P1 = JsonData["P1"] == null ? "" : JsonData["P1"].ToString();
                string P2 = JsonData["P2"] == null ? "" : JsonData["P2"].ToString();

                //Dictionary<string, string> results3 = JsonConvert.DeserializeObject<Dictionary<string, string>>(PostData.ToString());
                var function = Activator.CreateInstance(typeof(AuthManage)) as AuthManage;
                var method = function.GetType().GetMethod(Action.ToUpper());
                method.Invoke(function, new object[] { JsonData, Model, P1, P2, UserInfo });
                new JH_Auth_LogB().InsertLog(Model.Action, "--调用接口", "", UserInfo.User.UserName, UserInfo.User.UserRealName, UserInfo.QYinfo.ComId, "");
            }
            catch (Exception ex)
            {
                Model.ErrorMsg = Action + "接口调用失败,请检查日志";
                Model.Result = ex.ToString();
                new JH_Auth_LogB().InsertLog(Action, Model.ErrorMsg + ex.StackTrace.ToString(), ex.ToString(), tokenModel.UserName, "", 0, "");
            }

            return ControHelp.CovJson(Model);
        }





        /// <summary>
        /// 导入用户模板
        /// </summary>
        /// <param name="excelfile"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<string> IMPORTUSER(IFormFile excelfile)
        {
            try
            {
                var files = Request.Form.Files;
                var file = files.FirstOrDefault();
                using (var memoryStream = new MemoryStream())
                {

                    file.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    string headrow = Request.Form["headrow"].ToString();
                    string suffix = file.FileName.Split('.')[1];
                    Model.Result = new CommonHelp().ExcelToTable(memoryStream, int.Parse(headrow), suffix);
                }

            }
            catch (Exception ex)
            {
                Model.ErrorMsg = "IMPORTUSER导入失败";
                Model.Result = ex.ToString();
                new JH_Auth_LogB().InsertLog("IMPORTUSER", Model.ErrorMsg + ex.StackTrace.ToString(), ex.ToString(), "", "", 0, "");
                return "";
            }


            return ControHelp.CovJson(Model);
        }


        /// <summary>
        /// 导出员工数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<string> EXPORTYG()
        {
            var context = _accessor.HttpContext;
            var tokenHeader = context.Request.Cookies["szhlcode"].ToString().Replace("Bearer ", "");
            TokenModelJWT tokenModel = JwtHelper.SerializeJWT(tokenHeader);
            JH_Auth_UserB.UserInfo UserInfo = new JH_Auth_UserB().GetUserInfo(10334, tokenModel.UserName);


            JObject JsonData = new JObject();
            string P1 = context.Request.Query["P1"].ToString();
            string P2 = context.Request.Query["P2"].ToString();
            string pagecount = context.Request.Query["pagecount"].ToString();
            JsonData.Add("P1", P1);
            JsonData.Add("P2", P2);
            JsonData.Add("pagecount", pagecount);

            new AuthManage().GETUSERBYCODENEWPAGE(JsonData, Model, P1, P2, UserInfo);

            DataTable dt = Model.Result;

            string sqlCol = "ID,UserOrder|序号,DeptName|部门,RoomCode|房间号,UserName|账号,UserRealName|姓名,Sex|性别,mobphone|手机,QQ|QQ,weixinCard|微信,mailbox|邮箱,telphone|座机,ROLENAME|职务,Usersign|职责,UserGW|岗位,IDCard|身份证,HomeAddress|家庭住址";
            DataTable dtResult = dt.DelTableCol(sqlCol);
            HSSFWorkbook workbook = new HSSFWorkbook();
            workbook = CommonHelp.ExportToExcel(dtResult);
            var stream = new NPOIMemoryStream();
            workbook.Write(stream);
            stream.Flush();
            stream.Position = 0;
            return File(stream, "application/ms-excel", string.Format("{0}.xls", "员工_导出文件_" + DateTime.Now.Ticks));
        }




        //[HttpGet("download-file")]
        //public FileResult downloadFile(string name, string display)
        //{
        //    string contentType;
        //    contentTypeProvider.TryGetContentType(name, out contentType);
        //    HttpContext.Response.ContentType = contentType;
        //    string path = environment.WebRootPath + @"\images\" + name; // 注意哦, 不要像我这样直接使用客户端的值来拼接 path, 危险的 
        //    FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(path), contentType)
        //    {
        //        FileDownloadName = display
        //    };
        //    // return File("~/excels/report.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx"); // 返回 File + 路径也是可以, 这个路径是从 wwwroot 走起 
        //    // return File(await System.IO.File.ReadAllBytesAsync(path), same...) // 或则我们可以直接返回 byte[], 任意从哪里获取都可以. 

        //    return result;
        //}







    }
}
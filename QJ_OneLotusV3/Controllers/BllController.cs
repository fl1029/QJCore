using Aspose.Words;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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
using System.Reflection;

namespace QJ_OneLotusV3.Controllers
{
    /// <summary>
    /// 业务功能模块接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BllController : ControllerBase
    {
        private IHttpContextAccessor _accessor;
        private IHostingEnvironment hostingEnv { get; set; }
        private IContentTypeProvider contentTypeProvider { get; set; }


        Msg_Result Model = new Msg_Result() { Action = "", ErrorMsg = "" };


        public BllController(IHttpContextAccessor accessor, IHostingEnvironment env)
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


                // 1.Load(命名空间名称)，GetType(命名空间.类名)
                Type type = Assembly.Load("QJY.API").GetType("QJY.API." + Action.Split('_')[0].ToUpper()+ "Manage");
                //2.GetMethod(需要调用的方法名称)
                MethodInfo method = type.GetMethod(Action.Split('_')[1].ToUpper());
                // 3.调用的实例化方法（非静态方法）需要创建类型的一个实例
                object obj = Activator.CreateInstance(type);
                //4.方法需要传入的参数
                object[] parameters = new object[] { JsonData, Model, P1, P2, UserInfo };
                method.Invoke(obj, parameters);
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
        /// 外部接口，不需要验证权限
        /// </summary>
        /// <param name="Action">执行接口</param>
        /// <param name="PostData">数据</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<string> PubExeAction(string Action, Object PostData)
        {
          
            try
            {

                JObject JsonData = JObject.FromObject(PostData);
                string P1 = JsonData["P1"] == null ? "" : JsonData["P1"].ToString();
                string P2 = JsonData["P2"] == null ? "" : JsonData["P2"].ToString();
                var function = Activator.CreateInstance(typeof(PubManage)) as PubManage;
                var method = function.GetType().GetMethod(Action.ToUpper());
                method.Invoke(function, new object[] { JsonData, Model, P1, P2, null });
            }
            catch (Exception ex)
            {
                Model.ErrorMsg = Action + "接口调用失败,请检查日志";
                Model.Result = ex.ToString();
                new JH_Auth_LogB().InsertLog(Action, Model.ErrorMsg + ex.StackTrace.ToString(), ex.ToString(), "", "", 0, "");
            }

            return ControHelp.CovJson(Model);
        }


        /// <summary>
        ///导出统计数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<string> EXPORTBDTJDATA()
        {
            var context = _accessor.HttpContext;
            var tokenHeader = context.Request.Cookies["szhlcode"].ToString().Replace("Bearer ", "");
            TokenModelJWT tokenModel = JwtHelper.SerializeJWT(tokenHeader);
            JH_Auth_UserB.UserInfo UserInfo = new JH_Auth_UserB().GetUserInfo(10334, tokenModel.UserName);


            JObject JsonData = new JObject();
            string P1 = context.Request.Query["P1"].ToString();
            string P2 = context.Request.Query["P2"].ToString();
            string sdate = context.Request.Query["sdate"].ToString();
            string edate = context.Request.Query["edate"].ToString();
            JsonData.Add("P1", P1);
            JsonData.Add("P2", P2);
            JsonData.Add("sdate", sdate);
            JsonData.Add("edate", edate);

            new FORMBIManage().GETBDTJDATA(JsonData, Model, P1, P2, UserInfo);

            DataTable dt = Model.Result;

            string sqlCol = "CRUserName|发起人,CRDate|发起时间,BranchName|所在部门,";
            DataTable dtResult = dt.DelTableCol(sqlCol);
            HSSFWorkbook workbook = new HSSFWorkbook();
            workbook = CommonHelp.ExportToExcel(dtResult);
            var stream = new NPOIMemoryStream();
            workbook.Write(stream);
            stream.Flush();
            stream.Position = 0;
            return File(stream, "application/ms-excel", string.Format("{0}.xls", "导出文件_" + DateTime.Now.Ticks));




        }


        /// <summary>
        /// 按照模板导出表单Word
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<string> EXPORTWORD()
        {

            var context = _accessor.HttpContext;
            var tokenHeader = context.Request.Cookies["szhlcode"].ToString().Replace("Bearer ", "");
            TokenModelJWT tokenModel = JwtHelper.SerializeJWT(tokenHeader);
            JH_Auth_UserB.UserInfo UserInfo = new JH_Auth_UserB().GetUserInfo(10334, tokenModel.UserName);


            string P1 = context.Request.Query["P1"].ToString();
            string P2 = context.Request.Query["P2"].ToString();
            int pdid = 0;
            int.TryParse(P1, out pdid);

            int piid = 0;
            int.TryParse(P2, out piid);


            Yan_WF_PD PD = new Yan_WF_PDB().GetEntity(d => d.ID == pdid && d.ComId == UserInfo.User.ComId);
            Yan_WF_PI PI = new Yan_WF_PIB().GetEntity(d => d.ID == piid && d.ComId == UserInfo.User.ComId);

            if (PD.ExportFile == null)
            {
                return "";

            }
            int fileID = int.Parse(PD.ExportFile);
            FT_File MBFile = new FT_FileB().GetEntities(d => d.ID == fileID).FirstOrDefault();
            Dictionary<string, string> dictSource = new Dictionary<string, string>();

            List<JH_Auth_ExtendMode> ExtendModes = new List<JH_Auth_ExtendMode>();
            ExtendModes = new JH_Auth_ExtendModeB().GetEntities(D => D.ComId == UserInfo.User.ComId && D.PDID == pdid).ToList();
            foreach (JH_Auth_ExtendMode item in ExtendModes)
            {
                string strValue = new JH_Auth_ExtendDataB().GetFiledValue(item.TableFiledColumn, pdid, piid);
                dictSource.Add("qj_" + item.TableFiledColumn, strValue);
            }

            dictSource.Add("qj_CRUser", PI.CRUserName);
            dictSource.Add("qj_BranchName", PI.BranchName);
            dictSource.Add("qj_CRDate", PI.CRDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            dictSource.Add("qj_PINUM", PI.ID.ToString());


            List<Yan_WF_TI> tiModels = new Yan_WF_TIB().GetEntities(d => d.PIID == piid).ToList();
            for (int i = 0; i < tiModels.Count; i++)
            {
                dictSource.Add("qj_Task" + i + ".TaskUser", new JH_Auth_UserB().GetUserRealName(UserInfo.User.ComId.Value, tiModels[i].TaskUserID));
                dictSource.Add("qj_Task" + i + ".TaskUserView", tiModels[i].TaskUserView);
                if (tiModels[i].EndTime != null)
                {
                    dictSource.Add("qj_Task" + i + ".EndTime", tiModels[i].EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }
            string strFileUrl = UserInfo.QYinfo.FileServerUrl + "/" + UserInfo.QYinfo.QYCode + "/document/" + MBFile.zyid;
            Aspose.Words.Document doc = new Aspose.Words.Document(strFileUrl);

            //使用文本方式替换
            foreach (string name in dictSource.Keys)
            {
                doc.Range.Replace(name, dictSource[name]);
            }

            #region 使用书签替换模式


            #endregion
            string Filepath = hostingEnv.WebRootPath + "/Export/";
            string strFileName = PD.ProcessName + DateTime.Now.ToString("yyMMddHHss") + ".doc";

            doc.Save(Filepath + strFileName, Aspose.Words.Saving.DocSaveOptions.CreateSaveOptions(SaveFormat.Doc));
            return File("~/excels/report.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
        }



        /// <summary>
        /// 导入薪资单模板
        /// </summary>
        /// <param name="excelfile"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<string> IMPORTXZ(IFormFile excelfile)
        {
            try
            {
                var files = Request.Form.Files;
                var file = files.FirstOrDefault();
                using (var memoryStream = new MemoryStream())
                {

                    file.CopyTo(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    string suffix = file.FileName.Split('.')[1];
                    new XZGLManage().EXCELTOTABLEXZ(Model, memoryStream, suffix);
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


    }
}
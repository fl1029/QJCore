using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QJY.API;
using QJY.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace QJ_OneLotusV3.Controllers
{

    /// <summary>
    /// 上传下载文件相关接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private IHttpContextAccessor _accessor;
        private IHostingEnvironment hostingEnv;
        Msg_Result ReturnModel = new Msg_Result() { Action = "", ErrorMsg = "" };

        public FileController(IHostingEnvironment env, IHttpContextAccessor accessor)
        {
            this.hostingEnv = env;
            _accessor = accessor;

        }
        public struct Result
        {
            /// <summary>
            /// 表示图片是否已上传成功。
            /// </summary>
            public bool success;
            /// <summary>
            /// 自定义的附加消息。
            /// </summary>
            public string msg;
            /// <summary>
            /// 表示原始图片的保存地址。
            /// </summary>
            public string sourceUrl;
            /// <summary>
            /// 表示所有头像图片的保存地址，该变量为一个数组。
            /// </summary>
            public ArrayList avatarUrls;
        }


        /// <summary>
        /// 上传头像
        /// </summary>
        /// <param name="strUserName">用户名</param>
        /// <returns></returns>
        [HttpPost("{strUserName}")]
        public ActionResult<string> UPTX(string strUserName)
        {

            Result result = new Result();
            result.avatarUrls = new ArrayList();
            result.success = false;
            result.msg = "Failure!";
            var files = Request.Form.Files;
            //取服务器时间+8位随机码作为部分文件名，确保文件名无重复。
            string fileName = DateTime.Now.ToString("yyyyMMddhhmmssff") + UploadHelp.CreateRandomCode(8);
            int avatarNumber = 1;
            foreach (var file in files)
            {
                string fieldName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string filePath = hostingEnv.WebRootPath;
                if (!Directory.Exists(filePath + $@"\upload\tx\"))
                {
                    Directory.CreateDirectory(filePath);
                }
                if (fieldName == "__source")
                {
                    result.sourceUrl = string.Format("/upload/tx/tx{0}.jpg", fileName);
                    string fileFullName = filePath + result.sourceUrl;
                    using (FileStream fs = System.IO.File.Create(fileFullName))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                }
                //头像图片(file 域的名称：__avatar1,2,3...)。
                else
                {
                    string virtualPath = string.Format("/upload/tx/{0}_{1}.jpg", avatarNumber, strUserName);
                    result.avatarUrls.Add(virtualPath);
                    string fileFullName = filePath + virtualPath;
                    using (FileStream fs = System.IO.File.Create(fileFullName))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }
                    avatarNumber++;
                }

            }


            result.success = true;
            result.msg = "Success!";
            //返回图片的保存结果（返回内容为json字符串，可自行构造，该处使用Newtonsoft.Json构造）
            //  Response.Write(JsonConvert.SerializeObject(result));
            return Ok(result);


        }



        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<string> UPPIC()
        {
            var files = Request.Form.Files;
            long size = files.Sum(f => f.Length);

            if (size > 104857600)
            {
                ReturnModel.ErrorMsg = "大小不能超过100M";
            }

            List<string> filePathResultList = new List<string>();
            foreach (var file in files)
            {
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string filePath = hostingEnv.WebRootPath + $@"\upload\pic\";
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                string suffix = fileName.Split('.')[1];

                if (UploadHelp.GetFileType(suffix) != "pic")
                {
                    ReturnModel.ErrorMsg = "只能上传图片格式";
                }
                fileName = Guid.NewGuid() + "." + suffix;
                string fileFullName = filePath + fileName;
                using (FileStream fs = System.IO.File.Create(fileFullName))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                filePathResultList.Add("/upload/pic/"+ fileName);
            }
            ReturnModel.Result = filePathResultList;
            //返回图片的保存结果（返回内容为json字符串，可自行构造，该处使用Newtonsoft.Json构造）
            //  Response.Write(JsonConvert.SerializeObject(result));
            return Ok(ReturnModel);
        }


        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="fileID">文件ID</param>
        [HttpGet]
        public void DFile(string fileID)
        {
            if (!string.IsNullOrEmpty(fileID))
            {
                int fileId = int.Parse(fileID.Split(',')[0]);
                FT_File file = new FT_FileB().GetEntity(d => d.ID == fileId);

                JH_Auth_QY QYMODEL = new JH_Auth_QYB().GetALLEntities().FirstOrDefault();

                var context = _accessor.HttpContext;
                string width = context.Request.Query["width"].ToString();
                string height = context.Request.Query["height"].ToString();
                string filename = QYMODEL.FileServerUrl + "/" + QYMODEL.QYCode + "/document/" + file.zyid;
                if (width + height != "")
                {
                    filename = QYMODEL.FileServerUrl + "/" + QYMODEL.QYCode + "/document/image/" + file.zyid + (width + height != "" ? ("/" + width + "/" + height) : "");
                }
                context.Response.Redirect(filename);
            }
           
        }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UEditorNetCore;

namespace QJ_OneLotusV3.Controllers
{
    /// <summary>
    /// 百度编辑器相关接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UEditorController : ControllerBase
    {
        private UEditorService ue;
        public UEditorController(UEditorService ue)
        {
            this.ue = ue;
        }



        /// <summary>
        /// 处理百度编辑器信息
        /// </summary>
        [HttpGet]
        [HttpPost]
        public void Do()
        {
            ue.DoAction(HttpContext);
        }
    }
}
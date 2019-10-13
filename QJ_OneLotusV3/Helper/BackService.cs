using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QJY.Common;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QJ_OneLotusV3
{

    public class BackService : BackgroundService
    {
        public ILogger _logger;
        public BackService(ILogger<BackService> logger)
        {
            this._logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Random rd = new Random();
                    string strUrl = Appsettings.app("APITX");
                    HttpWebResponse ResponseDataXS = CommonHelp.CreateHttpResponse(strUrl, null, 0, "", null, "GET");
                    string Returndata = new StreamReader(ResponseDataXS.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                    await Task.Delay(5000, stoppingToken); //启动后5秒执行一次 (用于测试)
                }
            }
            catch (Exception ex)
            {
                CommonHelp.WriteLOG(ex.Message.ToString());

            }
        }
    }
}

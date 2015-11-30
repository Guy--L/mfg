using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Web;

namespace Tags.Models
{
    public class Restarter
    {
        private void bounce()
        {
            ServiceController svcController = new ServiceController(ConfigurationManager.AppSettings["KWRuntime"]);

            if (svcController != null)
            {
                try
                {
                    svcController.Stop();
                    svcController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
                    svcController.Start();
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }
        }

        public Restarter()
        {
            var path = Path.GetDirectoryName(ConfigurationManager.AppSettings["KWConfig"]);

            File.Copy(ConfigurationManager.AppSettings["KWConfig"], Path.Combine(path, "NJ" + DateTime.Now.ToString("yyyyMMddHHmm")));
            File.Copy(ConfigurationManager.AppSettings["KWTemp"], ConfigurationManager.AppSettings["KWConfig"], true);

            bounce();
        }
    }
}
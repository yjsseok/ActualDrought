using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UFRI.FrameWork
{
    public class GMLogHelper
    {
        private ILog logger;
        private static GMLogHelper instance = new GMLogHelper();
        private GMLogHelper()
        {
            log4net.Config.XmlConfigurator.Configure();
            logger = log4net.LogManager.GetLogger("logger"); 
        }

        public static ILog Logger
        {
            get { return instance.logger; }
        }

        public static void WriteLog(string message)
        {
            GMLogHelper.Logger.Info(message);
        }

        public static void WriteLog(Exception ex)
        {
            GMLogHelper.Logger.Error(Trace.CurrentMethodName, ex);
        }
    }
}

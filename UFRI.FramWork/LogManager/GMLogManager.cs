using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UFRI.FrameWork
{
    public static class GMLogManager
    {
        public static void ConfigureLogger(string filePath)
        {
            XmlConfigurator.Configure(new FileInfo(filePath));
        }

        public static void WriteEntry(String message)
        {
            ILog logger = GetLogger();
            logger.Info("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message);
        }

        private static ILog GetLogger()
        {
            StackTrace st = new StackTrace();
            MethodBase method = st.GetFrame(2).GetMethod();
            string methodName = method.Name;
            string declareType = method.DeclaringType.Name;
            string callingAssembly = method.DeclaringType.Assembly.FullName;

            return LogManager.GetLogger(callingAssembly + " - " + declareType + "." + methodName);
        }

        public static void WriteEntry(String message, EventLogEntryType type)
        {
            ILog logger = GetLogger();
            logger.Info("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message);
            //_eventLog.WriteEntry("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message, type);
        }

        public static void WriteEntry(String message, EventLogEntryType type, int eventID)
        {
            ILog logger = GetLogger();
            logger.Info("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message);
            //_eventLog.WriteEntry("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message, type, eventID);
        }

        public static void WriteEntry(String message, EventLogEntryType type, int eventID, short category)
        {
            ILog logger = GetLogger();
            logger.Info("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message);
            //_eventLog.WriteEntry("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message, type, eventID, category);
        }

        public static void WriteEntry(String message, EventLogEntryType type, int eventID, short category, byte[] rawData)
        {
            ILog logger = GetLogger();
            logger.Info("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message);
            //_eventLog.WriteEntry("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + message, type, eventID, category, rawData);
        }
    }
}

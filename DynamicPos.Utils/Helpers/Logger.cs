using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DynamicPos.Utils.Helpers
{

    /// <summary>
    /// Log işlemlerini gerçekleştiren Class
    /// </summary>
    public class Logger
    {

        /// <summary>
        /// Log işlemini gerçekleştirecek nesne
        /// </summary>
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger("DynamicPosLogger");

        /// <summary>
        /// Error Log'larını gerçekleştiren metot
        /// </summary>
        /// <param name="message">Hata mesajı</param>
        /// <param name="ex">hata</param>
        public static void Error(string message, Exception ex)
        {
            var st = new StackTrace(ex, true);

            StringBuilder logString = new StringBuilder();
            logString.AppendLine();
            logString.AppendLine("".PadLeft(100, '-'));
            logString.AppendLine(message);
            logString.AppendLine(ex.Message);
            if (ex.InnerException != null)
            {
                logString.AppendLine(ex.InnerException.Message);
                if (ex.InnerException.InnerException != null)
                    logString.AppendLine(ex.InnerException.InnerException.Message);

            }
            var frames = st.GetFrames();
            if (frames != null)
                foreach (var stackFrame in frames)
                {
                    if (stackFrame.GetFileLineNumber() == 0)
                    {
                        logString.AppendLine(stackFrame.ToString());
                        continue;
                    }

                    logString.Append("File:");
                    logString.Append(Path.GetFileName(stackFrame.GetFileName()));
                    logString.Append("@");
                    logString.Append(stackFrame.GetFileLineNumber());
                    logString.Append(":");
                    logString.AppendLine(stackFrame.GetFileColumnNumber().ToString());
                }

            logString.AppendLine(st.ToString());
            logString.AppendLine("".PadLeft(100, '-'));
            //string.Empty.PadLeft(50, '-');
            Debug.WriteLine(logString.ToString());
            Log.Error(logString.ToString());
        }

        /// <summary>
        /// Bilgi içerikli Log'ların gerçekleştiği metot
        /// </summary>
        /// <param name="message">Loglanacak mesaj</param>
        public static void Info(string message)
        {
            StringBuilder logString = new StringBuilder();
            logString.AppendLine();
            logString.AppendLine("".PadLeft(100, '-'));
            logString.AppendLine(message);
            logString.AppendLine("".PadLeft(100, '-'));
            Log.Info(logString);
        }
        
        /// <summary>
        /// Log Configure işlemini gerçekleştiren metot.
        /// </summary>
        public static void ConfigureLog()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}

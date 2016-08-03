using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Log
{
    public class LocalLogger : ILogger
    {
        private readonly string _LogPath;

        public LocalLogger(string logPath)
        {
            _LogPath = logPath;
        }

        public LocalLogger() : this(Environment.CurrentDirectory) { }

        public void Info(string service, string additional, string content)
        {
            Append("info", service, additional, content);
        }

        public void Info(string service, string additional, System.Exception ex)
        {
            Append("info", service, additional, ex.ToString());
        }

        public void Debug(string service, string additional, string content)
        {
            Append("debug", service, additional, content);
        }

        public void Debug(string service, string additional, System.Exception ex)
        {
            Append("debug", service, additional, ex.ToString());
        }

        public void Warn(string service, string additional, string content)
        {
            Append("warn", service, additional, content);
        }

        public void Warn(string service, string additional, System.Exception ex)
        {
            Append("warn", service, additional, ex.ToString());
        }

        public void Fail(string service, string additional, string content)
        {
            Append("fail", service, additional, content);
        }

        public void Fail(string service, string additional, System.Exception ex)
        {
            Append("fail", service, additional, ex.ToString());
        }

        public void Error(string service, string additional, string content)
        {
            Append("error", service, additional, content);
        }

        public void Error(string service, string additional, System.Exception ex)
        {
            Append("error", service, additional, ex.ToString());
        }

        private void Append(string type, string service, string additional, string info)
        {
            var filename = System.IO.Path.Combine(_LogPath, String.Format("{0}.log", service));
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("Type:{0};Time:{1}", type, DateTime.Now));
            sb.AppendLine(String.Format("Additional:{0}", additional));
            sb.AppendLine(info);
            sb.AppendLine(String.Empty);

            System.IO.File.AppendAllText(filename, sb.ToString(), Encoding.UTF8);
        }
    }
}

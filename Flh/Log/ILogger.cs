using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flh.Log
{
    public interface ILogger
    {
        void Info(string service, string additional, string content);
        void Info(string service, string additional, System.Exception ex);
        void Debug(string service, string additional, string content);
        void Debug(string service, string additional, System.Exception ex);
        void Warn(string service, string additional, string content);
        void Warn(string service, string additional, System.Exception ex);
        void Fail(string service, string additional, string content);
        void Fail(string service, string additional, System.Exception ex);
        void Error(string service, string additional, string content);
        void Error(string service, string additional, System.Exception ex);
    }

    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Fail = 2,
        Warn = 3,
        Debug = 4,
        Info = 5
    }

    public class LoggerResolver
    {
        private const string Default_Additional = "None";
        public static LoggerResolver Current;

        private ILogger m_Logger;
        public string ServiceName { get; private set; }

        public bool InfoEnabled { get; set; }
        public bool DebugEnabled { get; set; }
        public bool WarnEnabled { get; set; }
        public bool FailEnabled { get; set; }
        public bool ErrorEnabled { get; set; }
        public LogLevel Level { get; set; }

        private LoggerResolver()
        {
            InfoEnabled = true;
            DebugEnabled = true;
            WarnEnabled = true;
            FailEnabled = true;
            ErrorEnabled = true;
            Level = LogLevel.Info;
        }

        public void SetServiceName(string serviceName)
        {
            this.ServiceName = serviceName;
        }

        public void SetLogger(ILogger logger)
        {
            this.m_Logger = logger;
        }

        static LoggerResolver()
        {
            Current = new LoggerResolver();
        }

        private bool InLevel(LogLevel level)
        {
            return level <= Level;
        }

        public void Info(string additional, string content)
        {
            if (m_Logger != null && InfoEnabled && InLevel(LogLevel.Info))
                m_Logger.Info(ServiceName, additional, content);
        }

        public void Info(string additional, System.Exception ex)
        {
            if (m_Logger != null && InfoEnabled && InLevel(LogLevel.Info))
                m_Logger.Info(ServiceName, additional, ex);
        }

        public void Info(string content) { Info(Default_Additional, content); }
        public void Info(System.Exception ex) { Info(Default_Additional, ex); }

        public void Debug(string additional, string content)
        {
            if (m_Logger != null && DebugEnabled && InLevel(LogLevel.Debug))
                m_Logger.Debug(ServiceName, additional, content);
        }
        public void Debug(string additional, System.Exception ex)
        {
            if (m_Logger != null && DebugEnabled && InLevel(LogLevel.Debug))
                m_Logger.Debug(ServiceName, additional, ex);
        }
        public void Debug(string content) { Debug(Default_Additional, content); }
        public void Debug(System.Exception ex) { Debug(Default_Additional, ex); }

        public void Warn(string additional, string content)
        {
            if (m_Logger != null && WarnEnabled && InLevel(LogLevel.Warn))
                m_Logger.Warn(ServiceName, additional, content);
        }
        public void Warn(string additional, System.Exception ex)
        {
            if (m_Logger != null && WarnEnabled && InLevel(LogLevel.Warn))
                m_Logger.Warn(ServiceName, additional, ex);
        }
        public void Warn(string content) { Warn(Default_Additional, content); }
        public void Warn(System.Exception ex) { Warn(Default_Additional, ex); }

        public void Fail(string additional, string content)
        {
            if (m_Logger != null && FailEnabled && InLevel(LogLevel.Fail))
                m_Logger.Fail(ServiceName, additional, content);
        }
        public void Fail(string additional, System.Exception ex)
        {
            if (m_Logger != null && FailEnabled && InLevel(LogLevel.Fail))
                m_Logger.Fail(ServiceName, additional, ex);
        }
        public void Fail(string content) { Fail(Default_Additional, content); }
        public void Fail(System.Exception ex) { Fail(Default_Additional, ex); }

        public void Error(string additional, string content)
        {
            if (m_Logger != null && ErrorEnabled && InLevel(LogLevel.Error))
                m_Logger.Error(ServiceName, additional, content);
        }
        public void Error(string additional, System.Exception ex)
        {
            if (m_Logger != null && ErrorEnabled && InLevel(LogLevel.Error))
                m_Logger.Error(ServiceName, additional, ex);
        }
        public void Error(string content) { Error(Default_Additional, content); }
        public void Error(System.Exception ex) { Error(Default_Additional, ex); }
    }
}

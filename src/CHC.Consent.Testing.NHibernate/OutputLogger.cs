using System;
using NHibernate;
using Xunit.Abstractions;

namespace CHC.Consent.Testing.NHibernate
{
    public class OutputLogger : IInternalLogger
    {
        private readonly string name;
        private readonly ITestOutputHelper output;

        public OutputLogger(string name, ITestOutputHelper output)
        {
            this.name = name;
            this.output = output;
        }


        private void Log(string level, object message, Exception exception)
        {
            if (name.Contains("SQL"))
            {
                try
                {
                    output.WriteLine($"{level} {name} {message} {exception}");
                }
                catch (InvalidOperationException e) when (e.Message == "There is no currently active test.")
                {
                    //sometimes the logger is held onto beyond the lifetime of the test
                }
            }
        }

        private void LogError(object message, Exception exception = null) => Log("Error", message, exception);

        private static string Format(string format, params object[] args) => string.Format(format, args);

        public void Error(object message) => LogError(message);


        public void Error(object message, Exception exception) => LogError(message, exception);


        public void ErrorFormat(string format, params object[] args) => LogError(Format(format, args));

        private void LogFatal(object message, Exception exception = null) => Log("Fatal", message, exception);
        public void Fatal(object message) => LogFatal(message);
        public void Fatal(object message, Exception exception) => LogFatal(message, exception);

        private void LogDebug(object message, Exception exception = null) => Log("Debug", message, exception);
        public void Debug(object message) => LogDebug(message);
        public void Debug(object message, Exception exception) => LogDebug(message, exception);
        public void DebugFormat(string format, params object[] args) => LogDebug(Format(format, args));


        private void LogInfo(object message, Exception exception = null) => Log("Info", message, exception);
        public void Info(object message) => LogInfo(message);
        public void Info(object message, Exception exception) => LogInfo(message, exception);
        public void InfoFormat(string format, params object[] args) => LogInfo(Format(format, args));

        private void LogWarn(object message, Exception exception = null) => Log("Warn", message, exception);
        public void Warn(object message) => LogWarn(message);
        public void Warn(object message, Exception exception) => LogWarn(message, exception);
        public void WarnFormat(string format, params object[] args) => LogWarn(Format(format, args));

        public bool IsErrorEnabled { get; } = true;
        public bool IsFatalEnabled { get; } = true;
        public bool IsDebugEnabled { get; } = true;
        public bool IsInfoEnabled { get; } = true;
        public bool IsWarnEnabled { get; } = true;
    }
}
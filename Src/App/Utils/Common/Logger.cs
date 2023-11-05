using System.Text.RegularExpressions;
using Kusto.Cloud.Platform.Utils;
using Serilog;
using Serilog.Context;
using Serilog.Formatting.Json;

namespace Klipboard.Utils
{
    public static class Logger
    {
        public static Serilog.ILogger Log => s_log;
        private static readonly Serilog.Core.Logger s_log;
        private static readonly SerilogTraceListener.SerilogTraceListener s_traceListener;
        private static readonly Regex s_hstringMatcher = new Regex("(^| ){1}h['\"](\\\\|\\'|\\\"|[^\"'])*[\"']", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex s_sigMatcher = new Regex("&sig=[a-zA-Z0-9%]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        static Logger()
        {
#if DEBUG
            var logFileName = $"{AppConstants.ApplicationName}_DEBUG_.log";
#else
            var logFileName = $"{AppConstants.ApplicationName}_.log";
#endif
            var logPath = Path.Combine(OpSysHelper.AppFolderPath(), "Log", logFileName);

            // Create a Logger Instance
            s_log = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.File(
                    new JsonFormatter(renderMessage: true),
                    logPath, 
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(5))
                .CreateLogger();

            // Redirect C# traces to Serilog (Warning or higher)
            TraceSourceManager.SetTraceVerbosityForAll(TraceVerbosity.Warning);
            s_traceListener = new SerilogTraceListener.SerilogTraceListener(s_log);
            System.Diagnostics.Trace.Listeners.Add(s_traceListener);
        }

        public static IDisposable OperationScope(string className, string operationName, bool appendGuid = true)
        {
            var guidStr = appendGuid ? $";{Guid.NewGuid().ToString()}" : "";
            var scopeStr = $"{className};{operationName}{guidStr}";
            var scope = LogContext.PushProperty("OperationScope", scopeStr);

            return scope;
        }

        public static void CloseLog()
        {
            if (s_traceListener != null) 
            {
                s_traceListener.Flush();
                System.Diagnostics.Trace.Listeners.Remove(s_traceListener);
                s_traceListener.Dispose();
            }

            if (s_log != null)
            {
                s_log.Dispose();
            }
        }

        public static string ObfuscateHiddentStrings(string input)
        {
            input = s_hstringMatcher.Replace(input, "h'*****'");
            input = s_sigMatcher.Replace(input, "&sig=*****");

            return input;
        }
    }
}

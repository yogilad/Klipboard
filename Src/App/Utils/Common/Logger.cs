using Serilog;


namespace Klipboard.Utils
{
    public static class Logger
    {
        public static Serilog.ILogger Log => s_log;
        private static readonly Serilog.Core.Logger s_log;
        private static readonly SerilogTraceListener.SerilogTraceListener s_traceListener;

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
                .MinimumLevel.Verbose()
                .WriteTo.File(
                    logPath, 
                    rollingInterval: RollingInterval.Day, 
                    retainedFileCountLimit: 7,
                    buffered: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(5))
                .CreateLogger();

            // Redirect C# traces to Serilog
            s_traceListener = new SerilogTraceListener.SerilogTraceListener(s_log);
            System.Diagnostics.Trace.Listeners.Add(s_traceListener);
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
    }
}

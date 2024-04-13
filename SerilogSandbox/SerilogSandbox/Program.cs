using System.Diagnostics;
using Serilog;
using Serilog.Events;

namespace SerilogSandbox
{
    internal class Program
    {
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var logger = MyLoggerFactory.CreateLogger<Program>();

            logger.Verbose("Verbose!");
            logger.Debug("Debug!");
            logger.Information("Information!");
            logger.Warning("Warning!");
            logger.Error("Error!");
            logger.Fatal("Fatal!");
        }
    }

    public class MyLoggerFactory
    {
        public static ILogger CreateLogger<TSource>()
        {
            var baseLogger = Log.ForContext<TSource>();

            var myLogger = new MyLogger(baseLogger);

            return myLogger;
        }
    }

    public class MyLogger(ILogger baseLogger) : ILogger
    {
        /// <summary>
        /// これ以上のレベルのLogEventが発生したらDebugger.Break()を呼び出す。
        /// </summary>
        public LogEventLevel BreakIfGreaterThan { get; set; } = LogEventLevel.Error;

        // この属性を付けると、このメソッド内でデバッガーがステップインしない。
        // logger.Errorとか呼び出したところでブレークポイント貼られてたみたいな挙動になる。
        [DebuggerStepThrough]
        public void Write(LogEvent logEvent)
        {
            baseLogger.Write(logEvent);

            if (!Debugger.IsAttached)
            {
                return;
            }

            if (logEvent.Level >= BreakIfGreaterThan)
            {
                // DebuggerStepThroughがないとここでブレークする。ステップアウトが必要で不便。
                Debugger.Break();
            }
        }
    }
}

using System.Diagnostics;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

var stringValue = "中断したときに通常のブレークポイントと同じように値の中身が見れることを確認する用のローカル変数";
var intValue = 123;
var now = DateTime.Now; ;

var logger = MyLoggerFactory.CreateLogger<Program>();

logger.Verbose("{str} {int} {now}", stringValue, intValue, now);

logger.Verbose("Verbose!");
logger.Debug("Debug!");
logger.Information("Information!");
logger.Warning("Warning!このレベルはまだ中断されないよー");
logger.Error("Error!ブレークポイント張ったときのようにここで中断されるよー");
logger.Fatal("Fatal!ブレークポイント張ったときのようにここで中断されるよー");

class MyLoggerFactory
{
    public static ILogger CreateLogger<TSource>()
    {
        var baseLogger = Log.ForContext<TSource>();

        var myLogger = new MyLogger(baseLogger);

        return myLogger;
    }
}

class MyLogger(ILogger baseLogger) : ILogger
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

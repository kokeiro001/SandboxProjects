using Serilog;
using Serilog.Exceptions;

namespace OutputFormatSandbox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LogExceptionSandbox(false, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Exception}{Properties:j}{NewLine}");

            LogExceptionSandbox(true, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Exception}{Properties}{NewLine}");

            LogExceptionSandbox(true, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{Exception} {Properties:j}{NewLine}");
        }

        private static void LogExceptionSandbox(bool isEnableEnrichExceptionDetails, string outputTemplate, int dummyArrayIndex = 100)
        {

            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: outputTemplate);

            if (isEnableEnrichExceptionDetails)
            {
                loggerConfiguration = loggerConfiguration
                    .Enrich.WithExceptionDetails();
            }

            var logger = loggerConfiguration
                .CreateLogger();

            var contextLogger = logger.ForContext<Program>();

            contextLogger.Information("isEnableEnrichExceptionDetails: {isEnableEnrichExceptionDetails} outputTemplate: {outputTemplate}",
                isEnableEnrichExceptionDetails, outputTemplate);

            try
            {
                ArgumentOutOfRangeException.ThrowIfNotEqual(dummyArrayIndex, 0);
            }
            catch (Exception exception)
            {
                contextLogger.Error(exception, "検証エラー");
            }

        }
    }
}

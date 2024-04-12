using Serilog;

namespace SerilogSandbox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();


            var logger = Log.ForContext<Program>();

            logger.Information("Hello, World!");
        }
    }
}

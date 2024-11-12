using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace react_native_app_bk.Logging
{
    public class CustomConsoleFormatter : ConsoleFormatter
    {
        public CustomConsoleFormatter() : base("CustomFormatter") { }

        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider? scopeProvider,
            TextWriter textWriter)
        {
            var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
            if (message == null)
            {
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logLevel = logEntry.LogLevel;

            if (message.Contains("DbCommand"))
            {
                WriteDbCommand(message, timestamp);
            }
            else if (message.Contains("initialized"))
            {
                WriteWithColor($"[{timestamp}] DATABASE: Context Initialized", ConsoleColor.Cyan, true);
            }
            else
            {
                var color = logLevel switch
                {
                    LogLevel.Information => ConsoleColor.Green,
                    LogLevel.Warning => ConsoleColor.Yellow,
                    LogLevel.Error => ConsoleColor.Red,
                    LogLevel.Critical => ConsoleColor.DarkRed,
                    _ => ConsoleColor.White
                };

                WriteWithColor($"[{timestamp}] {logLevel}: {message}", color, true);

                if (logEntry.Exception != null)
                {
                    WriteWithColor($"[{timestamp}] {logEntry.Exception.GetType().Name}: {logEntry.Exception.Message}", ConsoleColor.Red, true);

                    if (logEntry.Exception.InnerException != null)
                    {
                        WriteWithColor($"Inner Exception Message: {logEntry.Exception.InnerException}", ConsoleColor.Red, true);
                    }
                }
            }
        }

        private static void WriteDbCommand(string message, string timestamp)
        {
            try
            {
                var parts = message.Split(new[] { "CommandTimeout='30']" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    var query = parts[1].Trim();
                    var parameters = message.Contains("Parameters=")
                        ? message.Split("Parameters=")[1].Split("],")[0] + "]"
                        : "None";

                    string logOutput = $@"[DATABASE QUERY] - {timestamp}
--------------------------------------------------------
Query:
{query}
    
Parameters: {parameters}
--------------------------------------------------------
";

                    WriteWithColor(logOutput, ConsoleColor.DarkCyan, false);
                }
            }
            catch
            {
                WriteWithColor($"[{timestamp}] DATABASE: {message}", ConsoleColor.DarkCyan, true);
            }
        }

        private static void WriteWithColor(string message, ConsoleColor color, bool newLine)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            if (newLine)
                Console.WriteLine(message);
            else
                Console.Write(message);

            Console.ForegroundColor = originalColor;
        }
    }
}

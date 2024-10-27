using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace react_native_app_bk.Logging
{
    public class CustomConsoleFormatter : ConsoleFormatter
    {
        public CustomConsoleFormatter() : base("CustomFormatter") { }

        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider scopeProvider,
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
                WriteDbCommand(message, timestamp, textWriter);
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
            }
        }

        private void WriteDbCommand(string message, string timestamp, TextWriter textWriter)
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

        private void WriteWithColor(string message, ConsoleColor color, bool newLine)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            if (newLine)
                Console.WriteLine(message);
            else
                Console.Write(message);

            Console.ForegroundColor = originalColor;
        }
        //    if (!TryGetMessage(in logEntry, out var message))
        //    {
        //        return;
        //    }

        //    var logLevel = logEntry.LogLevel;
        //    var category = logEntry.Category;
        //    var color = GetLogLevelColor(logLevel);
        //    var originalColor = Console.ForegroundColor;

        //    Console.ForegroundColor = color;

        //    if (category.Contains("EntityFrameworkCore"))
        //    {
        //        FormatEfCoreLog(message, textWriter);
        //    }
        //    else
        //    {
        //        FormatRegularLog(logLevel, message, textWriter);
        //    }

        //    Console.ForegroundColor = originalColor;
        //}

        //private static void FormatEfCoreLog(string message, TextWriter textWriter)
        //{
        //    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        //    if(message.Contains("Executed DbCommand"))
        //    {
        //        var parts = message.Split(new[] { "CommandType='Text', CommandTimeout='30']\n" }, StringSplitOptions.RemoveEmptyEntries);

        //        if (parts.Length > 1)
        //        {
        //            var query = parts[1].Trim();
        //            textWriter.WriteLine($@"

        //            [DATABASE QUERY] - {timestamp}
        //            ------------------------
        //            {query}
        //            ------------------------

        //            ");
        //        }
        //        else
        //        {
        //            textWriter.WriteLine($"[{timestamp}] DB: {message}");
        //        }
        //    }
        //    else if (message.Contains("initialized"))
        //    {
        //        textWriter.WriteLine($"[{timestamp}] DB Context Initialized");
        //    }
        //}

        //private static void FormatRegularLog(LogLevel logLevel, string message, TextWriter textWriter)
        //{
        //    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        //    textWriter.WriteLine($"[{timestamp}] {logLevel}: {message}");
        //}

        //private static ConsoleColor GetLogLevelColor(LogLevel logLevel) => logLevel switch 
        //{
        //        LogLevel.Trace => ConsoleColor.Gray,
        //        LogLevel.Debug => ConsoleColor.Gray,
        //        LogLevel.Information => ConsoleColor.Green,
        //        LogLevel.Warning => ConsoleColor.Yellow,
        //        LogLevel.Error => ConsoleColor.Red,
        //        LogLevel.Critical => ConsoleColor.Red,
        //        _ => ConsoleColor.Gray
        //};

        //private static bool TryGetMessage<TState>(
        //    in LogEntry<TState> logEntry,
        //    out string message)
        //{
        //    message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        //    return message != null;
        //}
    }
}

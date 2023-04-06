using System.Collections.Concurrent;
using System.Text;

namespace VChatService;
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
}

public enum LogOutputModel
{
    Console,
    File,
    Both,
}

public class VLogger
{
    private readonly LogLevel level;
    private readonly LogOutputModel output;
    private FileStream? fileStream;
    private readonly ConcurrentQueue<string> logs = new ConcurrentQueue<string>();
    public VLogger(string logFilePath, string minimumLogLevel, string logOutputMode)
    {
        level = (LogLevel)Enum.Parse(typeof(LogLevel), minimumLogLevel);
        output = (LogOutputModel)Enum.Parse(typeof(LogOutputModel), logOutputMode);
        if (output == LogOutputModel.File || output == LogOutputModel.Both)
        {
            string directory = Path.GetDirectoryName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFilePath))!;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            fileStream = new FileStream(Path.Combine(directory, $"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.txt"), FileMode.Create, FileAccess.Write);
        }
        Task.Run(() => ProcessLogsAsync());
    }
    ~VLogger()
    {
        fileStream?.Dispose();
    }
    private async Task ProcessLogsAsync()
    {
        while (true)
        {
            if (logs.TryDequeue(out string log))
            {
                await fileStream!.WriteAsync(Encoding.UTF8.GetBytes(log));
                await fileStream.FlushAsync();
            }
            else
            {
                await Task.Delay(100);
            }
        }
    }

    public void Write(LogLevel level, string message)
    {
        if (level >= this.level)
        {
            string log = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [{level.ToString()}] {message}{Environment.NewLine}";
            if (output == LogOutputModel.Console || output == LogOutputModel.Both)
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }
                Console.Write(log);
                Console.ResetColor();
            }
            if (output == LogOutputModel.File || output == LogOutputModel.Both)
            {
                logs.Enqueue(log);
            }
        }
    }

    public void Debug(string message)
    {
        Write(LogLevel.Debug, message);
    }

    public void Info(string message)
    {
        Write(LogLevel.Info, message);
    }

    public void Warning(string message)
    {
        Write(LogLevel.Warning, message);
    }

    public void Error(string message)
    {
        Write(LogLevel.Error, message);
    }
}
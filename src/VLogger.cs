using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Serialization;

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

public class VLoggerConfig
{
    [JsonPropertyName("log_file_path")]
    public string LogFilePath { get; set; } = "log";
    [JsonPropertyName("minimum_log_level")]
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;
    [JsonPropertyName("log_output_mode")]
    public LogOutputModel LogOutputMode { get; set; } = LogOutputModel.Both;
}

public class VLogger
{
    private FileStream? fileStream;
    private readonly ConcurrentQueue<string> logs = new ConcurrentQueue<string>();
    VLoggerConfig config;
    public VLogger(VLoggerConfig config)
    {
        this.config = config;
        if (config.LogOutputMode == LogOutputModel.File || config.LogOutputMode == LogOutputModel.Both)
        {
            string directory = Path.GetDirectoryName(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.LogFilePath))!;
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
            if (logs.TryDequeue(out string? log))
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
        if (level >= config.MinimumLogLevel)
        {
            string log = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [{level.ToString()}] {message}{Environment.NewLine}";
            if (config.LogOutputMode == LogOutputModel.Console || config.LogOutputMode == LogOutputModel.Both)
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
            if (config.LogOutputMode == LogOutputModel.File || config.LogOutputMode == LogOutputModel.Both)
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

    public void Debug(string tag, string message)
    {
        Write(LogLevel.Debug, $"[{tag}] {message}");
    }
    public void Info(string tag, string message)
    {
        Write(LogLevel.Info, $"[{tag}] {message}");
    }

    public void Warning(string tag, string message)
    {
        Write(LogLevel.Warning, $"[{tag}] {message}");
    }

    public void Error(string tag, string message)
    {
        Write(LogLevel.Error, $"[{tag}] {message}");
    }

    public void Debug(Type type, string message)
    {
        Write(LogLevel.Debug, $"[{type.Name}] {message}");
    }

    public void Info(Type type, string message)
    {
        Write(LogLevel.Info, $"[{type.Name}] {message}");
    }

    public void Warning(Type type, string message)
    {
        Write(LogLevel.Warning, $"[{type.Name}] {message}");
    }

    public void Error(Type type, string message)
    {
        Write(LogLevel.Error, $"[{type.Name}] {message}");
    }
}
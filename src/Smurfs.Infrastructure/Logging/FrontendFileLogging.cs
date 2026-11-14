using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace Smurfs.Infrastructure.Logging;

/// <summary>
/// Owns a frontend's rolling file logger from before host construction through shutdown.
/// </summary>
public sealed class FrontendFileLogging : IDisposable
{
    private readonly Logger _logger;

    /// <summary>
    /// Opens the rolling log for a frontend identified by a simple file name prefix.
    /// </summary>
    public FrontendFileLogging(string frontend)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(frontend);
        if (frontend.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))
        {
            throw new ArgumentException("Use only ASCII letters, digits, and hyphens.", nameof(frontend));
        }

        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Smurfs",
            "Logs");

        _logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(
                Path.Combine(directory, $"{frontend}-.log"),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 5 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 14,
                shared: true)
            .CreateLogger();
    }

    /// <summary>
    /// Adds the file provider to the host's existing logging providers.
    /// </summary>
    public void Configure(IConfiguration configuration, ILoggingBuilder logging)
    {
        logging.SetMinimumLevel(LogLevel.Warning);
        if (configuration["Logging:LogLevel:Smurfs"] is null)
        {
            logging.AddFilter("Smurfs", LogLevel.Information);
        }

        logging.AddSerilog(_logger, dispose: false);
    }

    /// <summary>
    /// Records a startup failure when the host logger may not be available.
    /// </summary>
    public void LogStartupFailure<T>(Exception exception)
        => _logger.ForContext<T>().Error(exception, "Frontend startup failed.");

    /// <inheritdoc />
    public void Dispose() => _logger.Dispose();
}

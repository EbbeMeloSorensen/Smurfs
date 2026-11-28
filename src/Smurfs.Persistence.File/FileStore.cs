using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Smurfs.Domain.VillageLife;
using IOFile = System.IO.File;

namespace Smurfs.Persistence.File;

internal sealed class FileStore
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter<Gender>(allowIntegerValues: false) }
    };

    private readonly string _path;

    public FileStore(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
    }

    public async Task<FileCatalog> ReadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await using var input = new FileStream(
                _path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete,
                4096, FileOptions.Asynchronous);
            var document = await JsonSerializer.DeserializeAsync<FileDocument>(input, _jsonOptions, cancellationToken);
            return FileCatalog.Restore(document ?? throw new InvalidDataException("The catalog is null."));
        }
        catch (FileNotFoundException)
        {
            return new FileCatalog();
        }
        catch (DirectoryNotFoundException)
        {
            return new FileCatalog();
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or NullReferenceException)
        {
            throw new InvalidDataException("The catalog file is invalid. It has not been replaced.", exception);
        }
    }

    public async Task InitializeAsync()
    {
        await using var fileLock = await LockAsync(CancellationToken.None);
        var catalog = await ReadAsync(CancellationToken.None);
        if (!IOFile.Exists(_path))
        {
            await WriteAsync(catalog.Capture(0), CancellationToken.None);
        }
    }

    public async Task SaveAsync(FileCatalog catalog, CancellationToken cancellationToken)
    {
        await using var fileLock = await LockAsync(cancellationToken);
        var current = await ReadAsync(cancellationToken);
        if (current.Revision != catalog.Revision)
        {
            throw new FileConflictException("The catalog changed in another session. Reload and retry.");
        }

        var nextRevision = checked(catalog.Revision + 1);
        await WriteAsync(catalog.Capture(nextRevision), cancellationToken);
        catalog.Revision = nextRevision;
    }

    private async Task<FileStream> LockAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var elapsed = Stopwatch.StartNew();
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                return new FileStream(_path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) when (elapsed.Elapsed < TimeSpan.FromSeconds(5))
            {
                await Task.Delay(25, cancellationToken);
            }
        }
    }

    private async Task WriteAsync(FileDocument document, CancellationToken cancellationToken)
    {
        var temporaryPath = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var output = new FileStream(
                temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                4096, FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(output, document, _jsonOptions, cancellationToken);
                await output.FlushAsync(cancellationToken);
                output.Flush(flushToDisk: true);
            }

            cancellationToken.ThrowIfCancellationRequested();
            IOFile.Move(temporaryPath, _path, overwrite: true);
        }
        finally
        {
            if (IOFile.Exists(temporaryPath))
            {
                IOFile.Delete(temporaryPath);
            }
        }
    }
}

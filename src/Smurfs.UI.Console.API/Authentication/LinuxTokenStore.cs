using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace Smurfs.UI.Console.API.Authentication;

/// <summary>
/// Retains tokens in owner-only files without requiring a desktop keyring.
/// File permissions provide access control; the token contents are not encrypted.
/// </summary>
[SupportedOSPlatform("linux")]
internal sealed class LinuxTokenStore(string directory) : ITokenStore
{
    private const UnixFileMode DirectoryMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

    private const UnixFileMode TokenMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;

    private readonly string _directory = Path.GetFullPath(directory);

    public string? Read(Uri address)
    {
        if (!ValidateDirectory())
        {
            return null;
        }

        var path = GetPath(address);
        if (!ValidateFile(path))
        {
            return null;
        }

        try
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
        catch (FileNotFoundException)
        {
            return null; // Another command can log out between validation and opening.
        }
    }

    public void Save(Uri address, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        RejectLink(new DirectoryInfo(_directory));
        Directory.CreateDirectory(_directory, DirectoryMode);
        ValidateDirectory();
        var path = GetPath(address);
        ValidateFile(path);
        var temporaryPath = Path.Combine(_directory, Guid.NewGuid().ToString("N") + ".tmp");
        var bytes = Encoding.UTF8.GetBytes(token);
        try
        {
            using (var output = new FileStream(temporaryPath, new FileStreamOptions
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.Write,
                Share = FileShare.None,
                UnixCreateMode = TokenMode
            }))
            {
                output.Write(bytes);
                output.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);
            File.Delete(temporaryPath);
        }
    }

    public void Delete(Uri address)
    {
        if (ValidateDirectory())
        {
            var path = GetPath(address);
            if (ValidateFile(path))
            {
                File.Delete(path);
            }
        }
    }

    internal static string ResolveDirectory(string? configHome, string userHome)
    {
        var root = !string.IsNullOrWhiteSpace(configHome) && Path.IsPathFullyQualified(configHome)
            ? configHome
            : Path.IsPathFullyQualified(userHome) ? Path.Combine(userHome, ".config")
            : throw new InvalidOperationException("Cannot determine the Linux credential directory. Configure an absolute XDG_CONFIG_HOME.");
        return Path.Combine(root, "smurfs-api");
    }

    private string GetPath(Uri address) => Path.Combine(_directory, TokenStoreKey.HashAddress(address) + ".token");

    private bool ValidateDirectory()
    {
        RejectLink(new DirectoryInfo(_directory));
        if (!Directory.Exists(_directory))
        {
            if (File.Exists(_directory))
            {
                throw new IOException("The Linux credential path is not a directory.");
            }

            return false;
        }

        if ((File.GetUnixFileMode(_directory) & ~DirectoryMode) != 0)
        {
            throw new IOException("The Linux credential directory must be accessible only to its owner (chmod 700).");
        }

        return true;
    }

    private static bool ValidateFile(string path)
    {
        RejectLink(new FileInfo(path));
        if (Directory.Exists(path))
        {
            throw new IOException("The Linux credential entry is not a file.");
        }

        if (!File.Exists(path))
        {
            return false;
        }

        if ((File.GetUnixFileMode(path) & ~TokenMode) != 0)
        {
            throw new IOException("The Linux credential file must be accessible only to its owner (chmod 600).");
        }

        return true;
    }

    private static void RejectLink(FileSystemInfo entry)
    {
        if (entry.LinkTarget is not null)
        {
            throw new IOException("Symbolic links are not allowed for Linux credential entries or their directory.");
        }
    }
}

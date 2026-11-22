using System.Runtime.Versioning;
using Smurfs.UI.Console.API.Authentication;

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Verifies the native Linux filesystem boundary; these cases are explicitly skipped elsewhere.
/// </summary>
[SupportedOSPlatform("linux")]
public sealed class LinuxTokenStoreTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "smurfs-linux-credentials-" + Guid.NewGuid().ToString("N"));

    private string CredentialDirectory => Path.Combine(_root, "credentials");

    /// <summary>
    /// New instances restore endpoint-scoped tokens, preserve private permissions, and remove only the selected entry.
    /// </summary>
    [LinuxFact]
    public void LinuxStore_RoundTripsWithOwnerOnlyPermissions()
    {
        var first = new Uri("https://example.com/");
        var other = new Uri("https://example.com:7443/base/");
        var store = new LinuxTokenStore(CredentialDirectory);
        store.Save(first, "first-token");
        store.Save(other, "other-token");
        Assert.Equal("first-token", new LinuxTokenStore(CredentialDirectory).Read(first));
        Assert.Null(store.Read(new Uri("http://example.com/")));
        Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute, File.GetUnixFileMode(CredentialDirectory));
        Assert.All(Directory.GetFiles(CredentialDirectory), file =>
            Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(file)));
        store.Save(first, "replacement-token");
        Assert.Equal("replacement-token", new LinuxTokenStore(CredentialDirectory).Read(first));
        Assert.Equal(2, Directory.GetFiles(CredentialDirectory).Length);
        store.Delete(first);
        Assert.Null(store.Read(first));
        Assert.Equal("other-token", store.Read(other));
        store.Delete(first);
        Assert.Empty(Directory.GetFiles(CredentialDirectory, "*.tmp"));
    }

    /// <summary>
    /// A missing login does not create directories during read or logout.
    /// </summary>
    [LinuxFact]
    public void LinuxStore_MissingReadAndLogoutDoNotCreateStorage()
    {
        var store = new LinuxTokenStore(CredentialDirectory);
        var address = new Uri("https://example.com/");
        Assert.Null(store.Read(address));
        store.Delete(address);
        Assert.False(Directory.Exists(_root));
    }

    /// <summary>
    /// Shared directories are refused without silently changing their access permissions.
    /// </summary>
    [LinuxFact]
    public void LinuxStore_RejectsSharedDirectory()
    {
        Directory.CreateDirectory(CredentialDirectory);
        File.SetUnixFileMode(CredentialDirectory, UnixFileMode.UserRead | UnixFileMode.UserWrite |
            UnixFileMode.UserExecute | UnixFileMode.OtherExecute);
        var store = new LinuxTokenStore(CredentialDirectory);
        Assert.Throws<IOException>(() => store.Save(new Uri("https://example.com/"), "token"));
        Assert.Empty(Directory.GetFiles(CredentialDirectory));
    }

    /// <summary>
    /// An overexposed token file is rejected on both read and replacement.
    /// </summary>
    [LinuxFact]
    public void LinuxStore_RejectsSharedTokenFile()
    {
        var store = new LinuxTokenStore(CredentialDirectory);
        var address = new Uri("https://example.com/");
        store.Save(address, "original-token");
        var file = Assert.Single(Directory.GetFiles(CredentialDirectory));
        File.SetUnixFileMode(file, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead);
        Assert.Throws<IOException>(() => store.Read(address));
        Assert.Throws<IOException>(() => store.Save(address, "replacement-token"));
        Assert.Equal("original-token", File.ReadAllText(file));
    }

    /// <summary>
    /// A token entry cannot redirect reads or writes through a symbolic link.
    /// </summary>
    [LinuxFact]
    public void LinuxStore_RejectsTokenSymlink()
    {
        var store = new LinuxTokenStore(CredentialDirectory);
        var address = new Uri("https://example.com/");
        store.Save(address, "original-token");
        var file = Assert.Single(Directory.GetFiles(CredentialDirectory));
        var target = Path.Combine(_root, "target");
        File.WriteAllText(target, "untouched");
        File.Delete(file);
        File.CreateSymbolicLink(file, target);
        Assert.Throws<IOException>(() => store.Read(address));
        Assert.Throws<IOException>(() => store.Save(address, "replacement-token"));
        Assert.Throws<IOException>(() => store.Delete(address));
        Assert.Equal("untouched", File.ReadAllText(target));
    }

    /// <summary>
    /// A linked credential directory is refused, including when its target has private permissions.
    /// </summary>
    [LinuxFact]
    public void LinuxStore_RejectsDirectorySymlink()
    {
        Directory.CreateDirectory(_root);
        var target = Path.Combine(_root, "target");
        Directory.CreateDirectory(target, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        Directory.CreateSymbolicLink(CredentialDirectory, target);
        var store = new LinuxTokenStore(CredentialDirectory);
        Assert.Throws<IOException>(() => store.Save(new Uri("https://example.com/"), "token"));
        Assert.Empty(Directory.GetFiles(target));
    }

    /// <summary>
    /// Invalid replacement input cannot change an existing token.
    /// </summary>
    [LinuxFact]
    public void LinuxStore_InvalidReplacementPreservesToken()
    {
        var store = new LinuxTokenStore(CredentialDirectory);
        var address = new Uri("https://example.com/");
        store.Save(address, "original-token");
        Assert.Throws<ArgumentException>(() => store.Save(address, " "));
        Assert.Equal("original-token", store.Read(address));
        Assert.Empty(Directory.GetFiles(CredentialDirectory, "*.tmp"));
    }

    /// <summary>
    /// XDG paths take precedence only when absolute; missing/relative values use the user configuration directory.
    /// </summary>
    [LinuxFact]
    public void LinuxStore_ResolvesXdgAndHomePaths()
    {
        Assert.Equal("/configured/smurfs-api", LinuxTokenStore.ResolveDirectory("/configured", "/home/test"));
        Assert.Equal("/home/test/.config/smurfs-api", LinuxTokenStore.ResolveDirectory(null, "/home/test"));
        Assert.Equal("/home/test/.config/smurfs-api", LinuxTokenStore.ResolveDirectory("relative", "/home/test"));
        Assert.Throws<InvalidOperationException>(() => LinuxTokenStore.ResolveDirectory(null, ""));
    }

    /// <summary>
    /// Removes only this test's uniquely named temporary directory.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}

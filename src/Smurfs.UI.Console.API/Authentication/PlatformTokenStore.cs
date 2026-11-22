namespace Smurfs.UI.Console.API.Authentication;

internal sealed class PlatformTokenStore : ITokenStore
{
    private ITokenStore? _store;

    public string? Read(Uri address) => GetStore().Read(address);

    public void Save(Uri address, string token) => GetStore().Save(address, token);

    public void Delete(Uri address) => GetStore().Delete(address);

    private ITokenStore GetStore()
    {
        if (_store is not null)
        {
            return _store;
        }

        if (OperatingSystem.IsWindows())
        {
            return _store = new WindowsTokenStore();
        }

        if (OperatingSystem.IsLinux())
        {
            var directory = LinuxTokenStore.ResolveDirectory(Environment.GetEnvironmentVariable("XDG_CONFIG_HOME"),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            return _store = new LinuxTokenStore(directory);
        }

        throw new PlatformNotSupportedException("Saved login supports Windows and Linux. Use SMURFS_API_TOKEN on other platforms.");
    }
}

namespace Smurf.Persistence.EFCore.AppData.UnitTest;

/// <summary>
/// Reports Linux-native integration cases as skipped on other operating systems.
/// </summary>
public sealed class LinuxFactAttribute : FactAttribute
{
    /// <summary>
    /// Selects cases only when native Linux filesystem semantics are available.
    /// </summary>
    public LinuxFactAttribute()
    {
        if (!OperatingSystem.IsLinux())
        {
            Skip = "Requires Linux filesystem permissions and symbolic-link semantics.";
        }
    }
}

/// <summary>
/// Reports Windows-native integration cases as skipped on other operating systems.
/// </summary>
public sealed class WindowsFactAttribute : FactAttribute
{
    /// <summary>
    /// Selects cases only when Windows Credential Manager is available.
    /// </summary>
    public WindowsFactAttribute()
    {
        if (!OperatingSystem.IsWindows())
        {
            Skip = "Requires Windows Credential Manager.";
        }
    }
}

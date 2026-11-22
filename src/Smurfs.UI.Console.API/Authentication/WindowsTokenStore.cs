using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Smurfs.UI.Console.API.Authentication;

internal sealed class WindowsTokenStore : ITokenStore
{
    private const uint GenericCredential = 1;

    private const int NotFound = 1168;

    public string? Read(Uri address)
    {
        EnsureSupported();
        if (!CredRead(GetTarget(address), GenericCredential, 0, out var pointer))
        {
            var error = Marshal.GetLastWin32Error();
            return error == NotFound ? null : throw new Win32Exception(error);
        }

        try
        {
            var credential = Marshal.PtrToStructure<NativeCredential>(pointer);
            var bytes = new byte[credential.CredentialBlobSize];
            try
            {
                Marshal.Copy(credential.CredentialBlob, bytes, 0, bytes.Length);
                return Encoding.UTF8.GetString(bytes);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(bytes);
            }
        }
        finally
        {
            CredFree(pointer);
        }
    }

    public void Save(Uri address, string token)
    {
        EnsureSupported();
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        var bytes = Encoding.UTF8.GetBytes(token);
        try
        {
            if (bytes.Length > 2560)
            {
                throw new InvalidOperationException("The API token exceeds the Windows credential size limit.");
            }

            var pointer = Marshal.AllocHGlobal(bytes.Length);
            try
            {
                Marshal.Copy(bytes, 0, pointer, bytes.Length);
                var credential = new NativeCredential
                {
                    Type = GenericCredential,
                    TargetName = GetTarget(address),
                    CredentialBlobSize = (uint) bytes.Length,
                    CredentialBlob = pointer,
                    Persist = 2, // CRED_PERSIST_LOCAL_MACHINE: same user, same machine, across logon sessions.
                    UserName = "Smurfs.API"
                };
                if (!CredWrite(ref credential, 0))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                CryptographicOperations.ZeroMemory(bytes);
                Marshal.Copy(bytes, 0, pointer, bytes.Length);
                Marshal.FreeHGlobal(pointer);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(bytes);
        }
    }

    public void Delete(Uri address)
    {
        EnsureSupported();
        if (!CredDelete(GetTarget(address), GenericCredential, 0))
        {
            var error = Marshal.GetLastWin32Error();
            if (error != NotFound)
            {
                throw new Win32Exception(error);
            }
        }
    }

    internal static string GetTarget(Uri address)
    {
        // Uri canonicalizes host names and default ports; preserve scheme, port, and base path isolation.
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(address.AbsoluteUri));
        return "Smurfs.API/" + Convert.ToHexString(digest);
    }

    private static void EnsureSupported()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Saved login currently requires Windows Credential Manager. Use SMURFS_API_TOKEN for commands on other platforms.");
        }
    }

    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredRead(string target, uint type, uint flags, out IntPtr credential);

    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredWrite(ref NativeCredential credential, uint flags);

    [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredDelete(string target, uint type, uint flags);

    [DllImport("advapi32.dll", EntryPoint = "CredFree")]
    private static extern void CredFree(IntPtr credential);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NativeCredential
    {
        public uint Flags;

        public uint Type;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? TargetName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? Comment;

        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;

        public uint CredentialBlobSize;

        public IntPtr CredentialBlob;

        public uint Persist;

        public uint AttributeCount;

        public IntPtr Attributes;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? TargetAlias;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? UserName;
    }
}

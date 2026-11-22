using System.Security.Cryptography;
using System.Text;

namespace Smurfs.UI.Console.API.Authentication;

internal static class TokenStoreKey
{
    internal static string HashAddress(Uri address) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(address.AbsoluteUri)));
}

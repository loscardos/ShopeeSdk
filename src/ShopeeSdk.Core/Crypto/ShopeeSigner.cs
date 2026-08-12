using System.Security.Cryptography;
using System.Text;

namespace Loscardos.ShopeeSdk.Core.Crypto;

public static class ShopeeSigner
{
    public static string Sign(string partnerKey, IReadOnlyList<string> parts)
    {
        var baseString = string.Concat(parts);
        var keyBytes = Encoding.UTF8.GetBytes(partnerKey);
        var baseBytes = Encoding.UTF8.GetBytes(baseString);
        var hash = HMACSHA256.HashData(keyBytes, baseBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

using System.Security.Cryptography;
using System.Text;

namespace HookSentry.Domain.Common;

public static class IngestToken
{
    public const string DestinationPrefix = "dst_";
    public const string SenderPrefix = "sndr_";

    public static (string RawToken, string Hash) Generate(string prefix)
    {
        var rawBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = prefix + Convert.ToHexString(rawBytes).ToLowerInvariant();
        return (rawToken, Hash(rawToken));
    }

    public static string Hash(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}

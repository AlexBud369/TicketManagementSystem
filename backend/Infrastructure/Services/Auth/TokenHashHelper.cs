using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services.Auth;

internal static class TokenHashHelper {
    public static string Hash(string token) {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}

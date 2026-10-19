using System.Security.Cryptography;
using System.Text;
using HelthTrack.Models;
using Microsoft.AspNetCore.Identity;

namespace HelthTrack.Services;

public sealed class PasswordService
{
    private const string Prefix = "HT1:";
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(User user, string password) => Prefix + _hasher.HashPassword(user, password);

    public bool Verify(User user, string password, out bool needsUpgrade)
    {
        needsUpgrade = false;
        if (user.Password.StartsWith(Prefix, StringComparison.Ordinal))
        {
            try
            {
                var result = _hasher.VerifyHashedPassword(user, user.Password[Prefix.Length..], password);
                needsUpgrade = result == PasswordVerificationResult.SuccessRehashNeeded;
                return result != PasswordVerificationResult.Failed;
            }
            catch (FormatException) { return false; }
        }

        // Preserve existing accounts; replace legacy plaintext on successful login.
        needsUpgrade = true;
        return CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(user.Password)),
            SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }
}

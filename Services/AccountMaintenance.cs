using HelthTrack.Data;
using HelthTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace HelthTrack.Services;

// Explicit operator commands; never run during normal web startup.
public static class AccountMaintenance
{
    public static async Task RunAsync(string command, IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataBaseContext>();
        var passwords = scope.ServiceProvider.GetRequiredService<PasswordService>();
        if (command == "create-doctor")
        {
            var username = configuration["Bootstrap:Username"]?.Trim();
            var password = configuration["Bootstrap:Password"];
            if (string.IsNullOrEmpty(username) || !System.Text.RegularExpressions.Regex.IsMatch(username, @"^[A-Za-z0-9_.-]{3,50}$") ||
                string.IsNullOrEmpty(password) || password.Length is < 12 or > 128)
                throw new InvalidOperationException("Set Bootstrap:Username and Bootstrap:Password (12–128 characters) using user secrets or environment variables.");
            if (await db.User.AnyAsync(u => u.Username == username) || await db.PatientUser.AnyAsync(u => u.Username == username))
                throw new InvalidOperationException("The requested username is unavailable. No account was modified.");
            var user = new User { Username = username, Role = "Doctor" };
            user.Password = passwords.Hash(user, password);
            db.User.Add(user);
            await db.SaveChangesAsync();
            Console.WriteLine("Doctor account created. Remove the Bootstrap credentials from local configuration.");
            return;
        }
        if (command == "upgrade-passwords")
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var users = await db.User.ToListAsync();
            var profiles = await db.PatientUser.ToListAsync();
            foreach (var user in users)
                if (!user.Password.StartsWith("HT1:", StringComparison.Ordinal))
                    user.Password = passwords.Hash(user, user.Password);
            foreach (var profile in profiles)
            {
                var account = users.FirstOrDefault(u => u.Username == profile.Username);
                if (account != null) profile.Password = account.Password;
                else if (!profile.Password.StartsWith("HT1:", StringComparison.Ordinal))
                    profile.Password = passwords.Hash(new User { Username = profile.Username }, profile.Password);
            }
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            Console.WriteLine("Existing account passwords upgraded. No credentials were printed.");
            return;
        }
        throw new InvalidOperationException("Unknown maintenance command. Use create-doctor or upgrade-passwords.");
    }
}

using HelthTrack.Data;
using HelthTrack.Models;
using HelthTrack.Services;
using HelthTrack.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace HelthTrack.Controllers;

[EnableRateLimiting("account")]
public class AccountController(DataBaseContext db, PasswordService passwords) : Controller
{
    [HttpGet]
    public IActionResult Register() => View(new RegisterPatientVM());

    [HttpPost]
    public async Task<IActionResult> RegisterPatient(RegisterPatientVM model)
    {
        model.Username = (model.Username ?? "").Trim();
        model.Email = (model.Email ?? "").Trim().ToLowerInvariant();
        model.FullName = (model.FullName ?? "").Trim();
        model.Disease = (model.Disease ?? "").Trim();
        if (model.Username.Equals("doctor", StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.Username), "This username is reserved.");
        if (!ModelState.IsValid) return View("Register", model);

        // Keep the two legacy account tables synchronized atomically.
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        if (await db.User.AnyAsync(u => u.Username == model.Username) ||
            await db.PatientUser.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
        {
            ModelState.AddModelError("", "This username or email is unavailable.");
            return View("Register", model);
        }
        var account = new User { Username = model.Username, Role = "Patient" };
        account.Password = passwords.Hash(account, model.Password);
        db.User.Add(account);
        db.PatientUser.Add(new PatientUser {
            Username = model.Username, Password = account.Password, FullName = model.FullName,
            Email = model.Email, DateOfBirth = model.DateOfBirth, Gender = model.Gender,
            Disease = model.Disease, Weight = 0, Height = 0,
        });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["Message"] = "Account created successfully. Please login.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginVM());

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM model)
    {
        if (!ModelState.IsValid) return View(model);
        var account = await db.User.FirstOrDefaultAsync(u => u.Username == model.Username.Trim());
        var upgrade = false;
        if (account == null || !passwords.Verify(account, model.Password, out upgrade) || account.Role != model.SelectedRole)
        {
            ViewBag.Message = "Invalid username, password, or account type.";
            return View(model);
        }
        var patient = await db.PatientUser.FirstOrDefaultAsync(p => p.Username == account.Username);
        if (upgrade) account.Password = passwords.Hash(account, model.Password);
        // Also retire the duplicate plaintext copy for existing patient accounts.
        if (patient != null) patient.Password = account.Password;
        await db.SaveChangesAsync();
        HttpContext.Session.Clear();
        HttpContext.Session.SetString("UserName", account.Username);
        HttpContext.Session.SetString("UserRole", account.Role);
        return account.Role == "Doctor" ? RedirectToAction("Dashboard_D", "Doctor") : RedirectToAction("Dashboard_P", "Patient");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}

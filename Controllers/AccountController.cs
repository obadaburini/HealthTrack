using HelthTrack.Data;
using HelthTrack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HelthTrack.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataBaseContext _db;

        public AccountController(DataBaseContext db)
        {
            _db = db;
        }

        // ================= REGISTER =================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPatient(PatientUser newPatient, string ConfirmPassword)
        {
            if (newPatient == null)
                return View("Register");

            // ✅ Trim important fields
            newPatient.FullName = (newPatient.FullName ?? "").Trim();
            newPatient.Email = (newPatient.Email ?? "").Trim();
            newPatient.Username = (newPatient.Username ?? "").Trim();
            newPatient.Password = (newPatient.Password ?? "").Trim();
            newPatient.Disease = (newPatient.Disease ?? "").Trim();
            newPatient.Gender = (newPatient.Gender ?? "").Trim();
            ConfirmPassword = (ConfirmPassword ?? "").Trim();

            // ✅ Required fields (حسب الفورم الحالي)
            // لاحظ: Age انشالت وصار عندك DateOfBirth
            if (string.IsNullOrWhiteSpace(newPatient.FullName) ||
                string.IsNullOrWhiteSpace(newPatient.Email) ||
                string.IsNullOrWhiteSpace(newPatient.Username) ||
                string.IsNullOrWhiteSpace(newPatient.Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword) ||
                string.IsNullOrWhiteSpace(newPatient.Disease) ||
                string.IsNullOrWhiteSpace(newPatient.Gender) ||
                newPatient.DateOfBirth == default ||
                newPatient.DateOfBirth > DateTime.Today)
            {
                ViewBag.Message = "Please fill all required fields correctly.";
                return View("Register", newPatient);
            }

            // ✅ Validate DOB -> age range (اختياري لكن مفيد)
            var age = DateTime.Today.Year - newPatient.DateOfBirth.Year;
            if (newPatient.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 0 || age > 120)
            {
                ViewBag.Message = "Please enter a valid date of birth.";
                return View("Register", newPatient);
            }

            // ✅ Password confirm
            if (newPatient.Password != ConfirmPassword)
            {
                ViewBag.Message = "Passwords do not match.";
                return View("Register", newPatient);
            }

            // ✅ Reserved username
            if (newPatient.Username.Equals("doctor", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Message = "This username is reserved.";
                return View("Register", newPatient);
            }

            // ✅ Unique username
            bool usernameTaken =
                await _db.PatientUser.AnyAsync(p => p.Username == newPatient.Username) ||
                await _db.User.AnyAsync(u => u.Username == newPatient.Username);

            if (usernameTaken)
            {
                ViewBag.Message = "Username already taken!";
                return View("Register", newPatient);
            }

            // ✅ Unique email (patients)
            bool emailTaken = await _db.PatientUser.AnyAsync(p => p.Email == newPatient.Email);
            if (emailTaken)
            {
                ViewBag.Message = "Email already used!";
                return View("Register", newPatient);
            }

            // ✅ IMPORTANT:
            // بما إن الوزن والطول مش موجودين بالفورم:
            // خليهم قيم افتراضية آمنة.
            // (الأفضل لاحقًا: تشيل Required عن Weight أو تضيفه في الفورم)
            if (newPatient.Weight == null || newPatient.Weight < 0) newPatient.Weight = 0;
            if (newPatient.Height == null || newPatient.Height < 0) newPatient.Height = 0;

            _db.PatientUser.Add(newPatient);

            _db.User.Add(new User
            {
                Username = newPatient.Username,
                Password = newPatient.Password,
                Role = "Patient"
            });

            await _db.SaveChangesAsync();

            TempData["Message"] = "Account created successfully. Please login.";
            return RedirectToAction("Login", "Account");
        }

        // ================= LOGIN =================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User user, string SelectedRole)
        {
            user.Username = (user.Username ?? "").Trim();
            user.Password = (user.Password ?? "").Trim();
            SelectedRole = (SelectedRole ?? "").Trim();

            var foundUser = await _db.User.FirstOrDefaultAsync(u =>
                u.Username == user.Username &&
                u.Password == user.Password);

            if (foundUser == null)
            {
                ViewBag.Message = "Invalid username or password.";
                return View(user);
            }

            if (!string.Equals(foundUser.Role, SelectedRole, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Message = "You selected the wrong account type.";
                return View(user);
            }

            HttpContext.Session.SetString("UserName", foundUser.Username);
            HttpContext.Session.SetString("UserRole", foundUser.Role);

            return foundUser.Role == "Doctor"
                ? RedirectToAction("Dashboard_D", "Doctor")
                : RedirectToAction("Dashboard_P", "Patient");
        }

        // ================= LOGOUT =================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}

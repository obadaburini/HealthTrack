using System;
using System.Linq;
using System.Threading.Tasks;
using HelthTrack.Data;
using HelthTrack.Models;
using HelthTrack.Models.Entities;
using HelthTrack.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelthTrack.Controllers
{
    public class PatientController : Controller
    {
        private readonly DataBaseContext _db;

        public PatientController(DataBaseContext db)
        {
            _db = db;
        }

        public IActionResult Dashboard_P()
        {
            return View();
        }

        // ================== DIABETES CLINIC ==================
        public IActionResult DiabetesClinic()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");
            return View();
        }

        // ================== BLOOD SUGAR ==================
        [HttpGet]
        public async Task<IActionResult> BloodSugar()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var userRecords = await _db.BloodSugarRecords
                .Where(x => x.Username == username)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            var last = userRecords.FirstOrDefault();

            ViewBag.LastSugar = last?.SugarLevel;
            ViewBag.Records = userRecords;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BloodSugar(double sugarLevel)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            _db.BloodSugarRecords.Add(new BloodSugarRecord
            {
                Username = username,
                SugarLevel = sugarLevel,
                Date = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return RedirectToAction("BloodSugar");
        }

        // ================== GENERAL CHECKUP ==================
        [HttpGet]
        public async Task<IActionResult> GeneralCheckup()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var userRecords = await _db.GeneralCheckupRecords
                .Where(x => x.Username == username)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            var last = userRecords.FirstOrDefault();

            ViewBag.Temperature = last?.Temperature;
            ViewBag.OxygenLevel = last?.OxygenLevel;
            ViewBag.RespiratoryRate = last?.RespiratoryRate;
            ViewBag.Records = userRecords;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GeneralCheckup(double temperature, double weight, double height, double oxygenLevel, double respiratoryRate)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            _db.GeneralCheckupRecords.Add(new GeneralCheckupRecord
            {
                Username = username,
                Temperature = temperature,
                Weight = weight,
                Height = height,
                OxygenLevel = oxygenLevel,
                RespiratoryRate = respiratoryRate,
                Date = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return RedirectToAction("GeneralCheckup");
        }

        // ================== APPOINTMENTS ==================
        [HttpGet]
        public async Task<IActionResult> Appointments(string from = "DiabetesClinic")
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            HttpContext.Session.SetString("ClinicName", from);
            ViewBag.ClinicName = from;

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var appointments = await _db.AppointmentRecords
                .Where(a => a.Username == username)
                .OrderBy(a => a.Date)
                .ToListAsync();

            ViewBag.Appointments = appointments;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAppointment(DateTime date, string description, string from)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            _db.AppointmentRecords.Add(new AppointmentRecord
            {
                Username = username,
                Date = date,
                Description = description ?? ""
            });

            await _db.SaveChangesAsync();

            from = string.IsNullOrWhiteSpace(from) ? "DiabetesClinic" : from;
            return RedirectToAction("Appointments", new { from });
        }

        // ================== HEART CLINIC ==================
        [HttpGet]
        public IActionResult HeartClinic()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> HeartVitals()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var userRecords = await _db.HeartRecords
                .Where(x => x.Username == username)
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            var last = userRecords.FirstOrDefault();

            ViewBag.Last = last;
            ViewBag.Records = userRecords;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> HeartVitals(int systolic, int diastolic, int heartRate)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            _db.HeartRecords.Add(new HeartRecord
            {
                Username = username,
                Systolic = systolic,
                Diastolic = diastolic,
                HeartRate = heartRate,
                Date = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return RedirectToAction("HeartVitals");
        }

        // ================== NUTRITION CLINIC ==================
        public IActionResult NutritionClinic()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NutritionResult(NutritionInputViewModel model)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View("NutritionClinic", model);

            var username = HttpContext.Session.GetString("UserName") ?? "";
            if (string.IsNullOrWhiteSpace(username))
                return RedirectToAction("Login", "Account");

            var patient = await _db.PatientUser.FirstOrDefaultAsync(p => p.Username == username);
            if (patient == null)
                return RedirectToAction("Login", "Account");

            int age = DateTime.Today.Year - patient.DateOfBirth.Year;
            if (patient.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 0 || age > 120)
            {
                ViewBag.Message = "Invalid date of birth in your profile. Please update it.";
                return View("NutritionClinic", model);
            }

            double heightInMeter = model.Height / 100.0;
            if (heightInMeter <= 0)
            {
                ViewBag.Message = "Invalid height.";
                return View("NutritionClinic", model);
            }

            double bmi = model.Weight / (heightInMeter * heightInMeter);

            string category;
            if (bmi < 18.5) category = "Underweight";
            else if (bmi < 25) category = "Normal weight";
            else if (bmi < 30) category = "Overweight";
            else category = "Obese";

            double activityMultiplier = 1.2;
            if (string.Equals(model.ActivityLevel, "Moderate", StringComparison.OrdinalIgnoreCase)) activityMultiplier = 1.55;
            else if (string.Equals(model.ActivityLevel, "High", StringComparison.OrdinalIgnoreCase)) activityMultiplier = 1.725;

            bool isMale = string.Equals(patient.Gender, "Male", StringComparison.OrdinalIgnoreCase);
            double bmr = (10 * model.Weight) + (6.25 * model.Height) - (5 * age) + (isMale ? 5 : -161);

            double tdee = bmr * activityMultiplier;

            double recommendedCalories = tdee;
            if (string.Equals(model.Goal, "Lose", StringComparison.OrdinalIgnoreCase))
                recommendedCalories = tdee - 500;
            else if (string.Equals(model.Goal, "Gain", StringComparison.OrdinalIgnoreCase))
                recommendedCalories = tdee + 300;

            if (recommendedCalories < 1200) recommendedCalories = 1200;

            // ✅ Macros (30/40/30)
            double calories = recommendedCalories;
            double proteinCalories = calories * 0.30;
            double carbsCalories = calories * 0.40;
            double fatCalories = calories * 0.30;

            double proteinGrams = proteinCalories / 4.0;
            double carbsGrams = carbsCalories / 4.0;
            double fatGrams = fatCalories / 9.0;

            var result = new NutritionResult
            {
                BMI = Math.Round(bmi, 2),
                Category = category,
                Age = age,
                Gender = patient.Gender,
                Goal = model.Goal,
                BMR = Math.Round(bmr, 0),
                DailyCalories = Math.Round(recommendedCalories, 0),

                ProteinGrams = Math.Round(proteinGrams, 0),
                CarbsGrams = Math.Round(carbsGrams, 0),
                FatGrams = Math.Round(fatGrams, 0)
            };

            ViewBag.Activity = model.ActivityLevel;

            return View(result);
        }

        public IActionResult ExerciseTips(string category)
        {
            ViewBag.Category = category;
            return View();
        }

        public IActionResult FoodTips(string category)
        {
            ViewBag.Category = category;
            return View();
        }

        // ================== BLOOD SUGAR - EDIT/DELETE ==================
        [HttpGet]
        public async Task<IActionResult> EditBloodSugar(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var rec = await _db.BloodSugarRecords
                .FirstOrDefaultAsync(x => x.Id == id && x.Username == username);

            if (rec == null) return NotFound();
            return View(rec);
        }

        [HttpPost]
        public async Task<IActionResult> EditBloodSugar(int id, double sugarLevel, DateTime date)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var rec = await _db.BloodSugarRecords
                .FirstOrDefaultAsync(x => x.Id == id && x.Username == username);

            if (rec == null) return NotFound();

            rec.SugarLevel = sugarLevel;
            rec.Date = date;

            await _db.SaveChangesAsync();
            return RedirectToAction("BloodSugar");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBloodSugar(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var rec = await _db.BloodSugarRecords
                .FirstOrDefaultAsync(x => x.Id == id && x.Username == username);

            if (rec == null) return NotFound();

            _db.BloodSugarRecords.Remove(rec);
            await _db.SaveChangesAsync();

            return RedirectToAction("BloodSugar");
        }

        // ================== HEART VITALS CRUD ==================
        [HttpGet]
        public async Task<IActionResult> EditHeartVitals(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var rec = await _db.HeartRecords
                .FirstOrDefaultAsync(x => x.Id == id && x.Username == username);

            if (rec == null) return NotFound();
            return View(rec);
        }

        [HttpPost]
        public async Task<IActionResult> EditHeartVitals(int id, int systolic, int diastolic, int heartRate, DateTime date)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var rec = await _db.HeartRecords
                .FirstOrDefaultAsync(x => x.Id == id && x.Username == username);

            if (rec == null) return NotFound();

            rec.Systolic = systolic;
            rec.Diastolic = diastolic;
            rec.HeartRate = heartRate;
            rec.Date = date;

            await _db.SaveChangesAsync();
            return RedirectToAction("HeartVitals");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHeartVitals(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var rec = await _db.HeartRecords
                .FirstOrDefaultAsync(x => x.Id == id && x.Username == username);

            if (rec == null) return NotFound();

            _db.HeartRecords.Remove(rec);
            await _db.SaveChangesAsync();

            return RedirectToAction("HeartVitals");
        }

        // ================== GENERAL CHECKUP CRUD ==================
        [HttpGet]
        public async Task<IActionResult> EditGeneralCheckup(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var rec = await _db.GeneralCheckupRecords
                .FirstOrDefaultAsync(x => x.Id == id && x.Username == username);

            if (rec == null) return NotFound();
            return View(rec);
        }

        [HttpPost]
        public async Task<IActionResult> EditGeneralCheckup(int id, double temperature, double weight, double height, double oxygenLevel, double respiratoryRate, DateTime date)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var rec = await _db.GeneralCheckupRecords
                .FirstOrDefaultAsync(x => x.Id == id && x.Username == username);

            if (rec == null) return NotFound();

            rec.Temperature = temperature;
            rec.Weight = weight;
            rec.Height = height;
            rec.OxygenLevel = oxygenLevel;
            rec.RespiratoryRate = respiratoryRate;
            rec.Date = date;

            await _db.SaveChangesAsync();
            return RedirectToAction("GeneralCheckup");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGeneralCheckup(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var rec = await _db.GeneralCheckupRecords
                .FirstOrDefaultAsync(x => x.Id == id && x.Username == username);

            if (rec == null) return NotFound();

            _db.GeneralCheckupRecords.Remove(rec);
            await _db.SaveChangesAsync();

            return RedirectToAction("GeneralCheckup");
        }

        // ================== APPOINTMENTS CRUD ==================
        [HttpGet]
        public async Task<IActionResult> EditAppointment(int id, string from = "DiabetesClinic")
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            ViewBag.From = from;

            var appt = await _db.AppointmentRecords
                .FirstOrDefaultAsync(a => a.Id == id && a.Username == username);

            if (appt == null) return NotFound();

            return View(appt);
        }

        [HttpPost]
        public async Task<IActionResult> EditAppointment(int id, DateTime date, string description, string from)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var appt = await _db.AppointmentRecords
                .FirstOrDefaultAsync(a => a.Id == id && a.Username == username);

            if (appt == null) return NotFound();

            appt.Date = date;
            appt.Description = description ?? "";

            await _db.SaveChangesAsync();

            from = string.IsNullOrWhiteSpace(from) ? "DiabetesClinic" : from;
            return RedirectToAction("Appointments", new { from });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAppointment(int id, string from)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Patient") return RedirectToAction("Login", "Account");

            var username = HttpContext.Session.GetString("UserName") ?? "";

            var appt = await _db.AppointmentRecords
                .FirstOrDefaultAsync(a => a.Id == id && a.Username == username);

            if (appt == null) return NotFound();

            _db.AppointmentRecords.Remove(appt);
            await _db.SaveChangesAsync();

            from = string.IsNullOrWhiteSpace(from) ? "DiabetesClinic" : from;
            return RedirectToAction("Appointments", new { from });
        }
    }
}

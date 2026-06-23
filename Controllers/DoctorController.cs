using System;
using System.Linq;
using System.Threading.Tasks;
using HelthTrack.Data;
using HelthTrack.Models;
using HelthTrack.Models.Entities;
using HelthTrack.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace HelthTrack.Controllers
{
    public class DoctorController : Controller
    {
        private readonly DataBaseContext _db;

        public DoctorController(DataBaseContext db)
        {
            _db = db;
        }

        // ================= AUTH =================
        private bool IsDoctor()
        {
            return HttpContext.Session.GetString("UserRole") == "Doctor";
        }

        private IActionResult DoctorOnly()
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            return null!;
        }

        // ================= DASHBOARD =================
        public IActionResult Dashboard_D()
        {
            var guard = DoctorOnly();
            if (guard != null) return guard;

            return View();
        }

        // ================= PATIENTS =================
        public async Task<IActionResult> Patients()
        {
            var guard = DoctorOnly();
            if (guard != null) return guard;

            var patients = await _db.Patient
                .OrderBy(p => p.PatientUserId)
                .ToListAsync();

            return View(patients);
        }

        // ================= DETAILS =================
        public async Task<IActionResult> Details(int id)
        {
            var guard = DoctorOnly();
            if (guard != null) return guard;

            var patient = await _db.Patient
                .Include(p => p.PatientUser)
                .FirstOrDefaultAsync(p => p.PatientUserId == id);

            if (patient == null) return NotFound();

            var username = patient.PatientUser?.Username ?? "";

            var vm = new DoctorPatientReportVM
            {
                Patient = patient,
                BloodSugar = await _db.BloodSugarRecords.Where(x => x.Username == username).OrderByDescending(x => x.Date).ToListAsync(),
                Checkups = await _db.GeneralCheckupRecords.Where(x => x.Username == username).OrderByDescending(x => x.Date).ToListAsync(),
                HeartVitals = await _db.HeartRecords.Where(x => x.Username == username).OrderByDescending(x => x.Date).ToListAsync(),
                Appointments = await _db.AppointmentRecords.Where(x => x.Username == username).OrderByDescending(x => x.Date).ToListAsync()
            };

            return View(vm);
        }

        // ================= ADD PATIENT =================
        [HttpGet]
        public async Task<IActionResult> AddPatient()
        {
            var guard = DoctorOnly();
            if (guard != null) return guard;

            ViewBag.PatientUsers = await _db.PatientUser
                .OrderBy(u => u.Username)
                .Select(u => new { u.Id, u.Username, u.FullName })
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPatient(Patient newPatient)
        {
            var guard = DoctorOnly();
            if (guard != null) return guard;

            var allowedTypes = new[] { "Diabetes", "Heart", "Both" };

            if (!string.IsNullOrWhiteSpace(newPatient.PatientType))
                newPatient.PatientType = newPatient.PatientType.Trim();

            if (string.IsNullOrWhiteSpace(newPatient.PatientType) || !allowedTypes.Contains(newPatient.PatientType))
                ModelState.AddModelError("PatientType", "Please select a valid patient type.");

            var user = await _db.PatientUser
                .Where(u => u.Id == newPatient.PatientUserId)
                .Select(u => new { u.FullName, u.DateOfBirth, u.Weight })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                ModelState.AddModelError("PatientUserId", "Selected patient account not found.");
            }
            else
            {
                newPatient.Name = user.FullName;
                newPatient.Age = CalculateAge(user.DateOfBirth);
                newPatient.Weight = user.Weight.HasValue && user.Weight.Value > 0 ? user.Weight.Value : 0;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PatientUsers = await _db.PatientUser
                    .OrderBy(u => u.Username)
                    .Select(u => new { u.Id, u.Username, u.FullName })
                    .ToListAsync();

                return View(newPatient);
            }

            _db.Patient.Add(newPatient);
            await _db.SaveChangesAsync();

            return RedirectToAction("Patients");
        }

        // ================= EDIT PATIENT =================
        [HttpGet]
        public async Task<IActionResult> EditPatient(int id)
        {
            var guard = DoctorOnly();
            if (guard != null) return guard;

            var patient = await _db.Patient.FirstOrDefaultAsync(p => p.PatientUserId == id);
            if (patient == null) return NotFound();

            var vm = new EditPatientVM
            {
                PatientUserId = patient.PatientUserId,
                PatientType = patient.PatientType ?? "Diabetes"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(EditPatientVM vm)
        {
            var guard = DoctorOnly();
            if (guard != null) return guard;

            if (!ModelState.IsValid) return View(vm);

            var patient = await _db.Patient.FirstOrDefaultAsync(p => p.PatientUserId == vm.PatientUserId);
            if (patient == null) return NotFound();

            patient.PatientType = vm.PatientType.Trim();
            await _db.SaveChangesAsync();

            return RedirectToAction("Patients");
        }

        // ================= DELETE PATIENT =================
        [HttpGet]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _db.Patient.FirstOrDefaultAsync(p => p.PatientUserId == id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeletePatient")]
        public async Task<IActionResult> DeletePatientConfirmed(int id)
        {
            var patient = await _db.Patient.FirstOrDefaultAsync(p => p.PatientUserId == id);
            if (patient == null) return NotFound();

            _db.Patient.Remove(patient);
            await _db.SaveChangesAsync();

            return RedirectToAction("Patients");
        }
        // ================= REPORTS =================
        public async Task<IActionResult> Reports()
        {
            var guard = DoctorOnly();
            if (guard != null) return guard;

            ViewBag.TotalPatients = await _db.Patient.CountAsync();

            ViewBag.DiabetesCount = await _db.Patient.CountAsync(p =>
                p.PatientType == "Diabetes" || p.PatientType == "Both");

            ViewBag.HeartCount = await _db.Patient.CountAsync(p =>
                p.PatientType == "Heart" || p.PatientType == "Both");

            ViewBag.BothCount = await _db.Patient.CountAsync(p =>
                p.PatientType == "Both");

            ViewBag.AvgAge = await _db.Patient.AnyAsync()
                ? await _db.Patient.AverageAsync(p => p.Age)
                : 0;

            // ❌ لا يوجد AvgWeight نهائيًا
            return View();
        }


        // ================= HELPERS =================
        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age < 0 ? 0 : age;
        }
    }
}

using System.Collections.Generic;
using HelthTrack.Models;
using HelthTrack.Models.Entities;

namespace HelthTrack.Models.ViewModels
{
    public class DoctorPatientReportVM
    {
        public Patient Patient { get; set; } = new Patient();

        public List<BloodSugarRecord> BloodSugar { get; set; } = new();
        public List<GeneralCheckupRecord> Checkups { get; set; } = new();
        public List<HeartRecord> HeartVitals { get; set; } = new();
        public List<AppointmentRecord> Appointments { get; set; } = new();
    }
}

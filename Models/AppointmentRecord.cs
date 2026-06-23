using System;

namespace HelthTrack.Models
{
    public class AppointmentRecord
    {
        public int Id { get; set; }                 // ✅ Primary Key
        public string Username { get; set; } = "";  // ✅
        public DateTime Date { get; set; }          // ✅
        public string Description { get; set; } = ""; // ✅
    }
}

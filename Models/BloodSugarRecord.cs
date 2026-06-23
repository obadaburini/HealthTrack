using System;

namespace HelthTrack.Models
{
    public class BloodSugarRecord
    {
        public int Id { get; set; }                 // ✅ PK للـ CRUD
        public string Username { get; set; } = "";  // ✅ لازم public set
        public DateTime Date { get; set; }          // ✅ وقت الإدخال
        public double SugarLevel { get; set; }      // ✅ القراءة
    }
}

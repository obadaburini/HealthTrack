using System;

namespace HelthTrack.Models
{
    public class HeartRecord
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public DateTime Date { get; set; }

        public int Systolic { get; set; }
        public int Diastolic { get; set; }
        public int HeartRate { get; set; }
    }
}

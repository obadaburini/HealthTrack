using System;

namespace HelthTrack.Models
{
    public class GeneralCheckupRecord
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public DateTime Date { get; set; }

        public double Temperature { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public double OxygenLevel { get; set; }
        public double RespiratoryRate { get; set; }
    }
}


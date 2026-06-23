namespace HelthTrack.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public DateTime Date { get; set; }
        public string? Note { get; set; }
    }
}

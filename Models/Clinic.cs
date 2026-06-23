namespace HelthTrack.Models.Entities
{
    public class Clinic
    {

        public int ClinicID { get; set; }
        public string Name { get; set; } = "";
        public string Specialty { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
    }
}
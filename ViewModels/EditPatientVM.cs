namespace HelthTrack.Models.ViewModels
{
    public class EditPatientVM
    {
        public int PatientUserId { get; set; }
        [System.ComponentModel.DataAnnotations.RegularExpression("Diabetes|Heart|Both")]
        public string PatientType { get; set; } = "Diabetes";
    }
}

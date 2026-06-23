namespace HelthTrack.Models
{
    public class Patient
    {
        public int Id { get; set; }

        // مفتاح الربط
        public int PatientUserId { get; set; }

        // Navigation Property
        public PatientUser? PatientUser { get; set; }

        // الاسم رح يتعبّى تلقائيًا من PatientUser.FullName عند الحفظ
        public string Name { get; set; } = "";

        public int Age { get; set; }
        public double Weight { get; set; }

        // ✅ بدل Disease
        public string PatientType { get; set; } = "";
    }
}

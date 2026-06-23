using HelthTrack.Models;
using HelthTrack.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelthTrack.Data
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options) { }

        public DbSet<User> User { get; set; }
        public DbSet<Patient> Patient { get; set; }
        public DbSet<PatientUser> PatientUser { get; set; }

        public DbSet<BloodSugarRecord> BloodSugarRecords { get; set; }
        public DbSet<GeneralCheckupRecord> GeneralCheckupRecords { get; set; }
        public DbSet<HeartRecord> HeartRecords { get; set; }
        public DbSet<AppointmentRecord> AppointmentRecords { get; set; }
    }
}

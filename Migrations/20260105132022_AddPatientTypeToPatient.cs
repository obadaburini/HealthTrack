using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelthTrack.Migrations
{
    public partial class AddPatientTypeToPatient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The preceding migration already renames Disease to PatientType.
            // Retain this migration ID for existing deployments without adding twice.
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Patient', N'PatientType') IS NULL
                    ALTER TABLE [dbo].[Patient] ADD [PatientType] nvarchar(max) NOT NULL DEFAULT N'';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // PatientType belongs to the preceding migration. Its Down renames it
            // back to Disease; removing it here would break rollback and lose data.
        }
    }
}

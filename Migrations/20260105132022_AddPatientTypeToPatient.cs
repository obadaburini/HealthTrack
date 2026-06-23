using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelthTrack.Migrations
{
    public partial class AddPatientTypeToPatient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PatientType",
                table: "Patient",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientType",
                table: "Patient");
        }
    }
}

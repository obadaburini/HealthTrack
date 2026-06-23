using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelthTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "PatientUser",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "PatientUser");
        }
    }
}

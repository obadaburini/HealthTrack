using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelthTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientUserIdToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Disease",
                table: "Patient");

            migrationBuilder.RenameColumn(
                name: "PatientID",
                table: "Patient",
                newName: "PatientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_PatientUserId",
                table: "Patient",
                column: "PatientUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patient_PatientUser_PatientUserId",
                table: "Patient",
                column: "PatientUserId",
                principalTable: "PatientUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patient_PatientUser_PatientUserId",
                table: "Patient");

            migrationBuilder.DropIndex(
                name: "IX_Patient_PatientUserId",
                table: "Patient");

            migrationBuilder.RenameColumn(
                name: "PatientUserId",
                table: "Patient",
                newName: "PatientID");

            migrationBuilder.AddColumn<string>(
                name: "Disease",
                table: "Patient",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

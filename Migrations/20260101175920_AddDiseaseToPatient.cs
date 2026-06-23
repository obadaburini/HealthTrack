using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelthTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddDiseaseToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Disease",
                table: "Patient",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Disease",
                table: "Patient");
        }
    }
}

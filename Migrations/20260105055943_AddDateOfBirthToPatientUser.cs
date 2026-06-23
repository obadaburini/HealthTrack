using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelthTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddDateOfBirthToPatientUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "PatientUser");

            migrationBuilder.AlterColumn<double>(
                name: "Height",
                table: "PatientUser",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "PatientUser",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "PatientUser");

            migrationBuilder.AlterColumn<double>(
                name: "Height",
                table: "PatientUser",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "PatientUser",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

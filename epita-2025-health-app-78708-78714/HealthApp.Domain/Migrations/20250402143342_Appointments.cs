using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class Appointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorFirstName",
                table: "Appointments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PatientFirstName",
                table: "Appointments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "Appointments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorFirstName",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PatientFirstName",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "Appointments");
        }
    }
}

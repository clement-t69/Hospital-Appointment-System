using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HealthApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "261cde6f-3894-4be8-a518-d575a6c2cfcd");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "36bde1c6-fbe5-4ed7-8bd0-5e1f5eafdabe");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c817615a-30c2-4808-a992-6c50144abac6");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "your_admin_guid", null, "Admin", "ADMIN" },
                    { "your_doctor_guid", null, "Doctor", "DOCTOR" },
                    { "your_patient_guid", null, "Patient", "PATIENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "your_admin_guid");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "your_doctor_guid");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "your_patient_guid");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "261cde6f-3894-4be8-a518-d575a6c2cfcd", null, "Doctor", "DOCTOR" },
                    { "36bde1c6-fbe5-4ed7-8bd0-5e1f5eafdabe", null, "Patient", "PATIENT" },
                    { "c817615a-30c2-4808-a992-6c50144abac6", null, "Admin", "ADMIN" }
                });
        }
    }
}

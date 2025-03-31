using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class PrettiestDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_AspNetUsers_DoctorId",
                table: "MedicalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_AspNetUsers_PatientId",
                table: "MedicalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_DoctorId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_PatientId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_DoctorId",
                table: "Prescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_AspNetUsers_PatientId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "MedicalHistories");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Doctors_DoctorId",
                table: "MedicalHistories",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_Doctors_DoctorId",
                table: "Prescriptions",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Doctors_DoctorId",
                table: "MedicalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Doctors_DoctorId",
                table: "Prescriptions");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "MedicalHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_AspNetUsers_DoctorId",
                table: "MedicalHistories",
                column: "DoctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_AspNetUsers_PatientId",
                table: "MedicalHistories",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_DoctorId",
                table: "Notifications",
                column: "DoctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_PatientId",
                table: "Notifications",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_DoctorId",
                table: "Prescriptions",
                column: "DoctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_AspNetUsers_PatientId",
                table: "Prescriptions",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

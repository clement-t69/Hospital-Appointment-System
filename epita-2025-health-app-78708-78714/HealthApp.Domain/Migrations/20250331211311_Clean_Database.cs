using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthApp.Domain.Migrations
{
    /// <inheritdoc />
    public partial class Clean_Database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_DoctorSpecializations_SpecializationId",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Doctors_DoctorId",
                table: "MedicalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_Doctors_DoctorId",
                table: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Administrators");

            migrationBuilder.DropTable(
                name: "DoctorAvailabilities");

            migrationBuilder.DropTable(
                name: "DoctorSpecializations");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_SpecializationId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "SpecializationId",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "Treatment",
                table: "MedicalHistories",
                newName: "Specialization");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Prescriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DoctorLastName",
                table: "MedicalHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "MedicalHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PatientLastName",
                table: "MedicalHistories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Specialization",
                table: "Doctors",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DoctorLastName",
                table: "Appointments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PatientLastName",
                table: "Appointments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Appointments",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "Date",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "DoctorLastName",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "PatientLastName",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "Specialization",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "DoctorLastName",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PatientLastName",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "Specialization",
                table: "MedicalHistories",
                newName: "Treatment");

            migrationBuilder.AddColumn<int>(
                name: "SpecializationId",
                table: "Doctors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Administrators",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administrators", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Administrators_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DoctorId = table.Column<string>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorAvailabilities_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorSpecializations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSpecializations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_SpecializationId",
                table: "Doctors",
                column: "SpecializationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilities_DoctorId",
                table: "DoctorAvailabilities",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_DoctorSpecializations_SpecializationId",
                table: "Doctors",
                column: "SpecializationId",
                principalTable: "DoctorSpecializations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    }
}

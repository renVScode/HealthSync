using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthSync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceOfferingToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceOfferingId",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceOfferingId",
                table: "Appointments",
                column: "ServiceOfferingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_DoctorServiceOfferings_ServiceOfferingId",
                table: "Appointments",
                column: "ServiceOfferingId",
                principalTable: "DoctorServiceOfferings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_DoctorServiceOfferings_ServiceOfferingId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ServiceOfferingId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ServiceOfferingId",
                table: "Appointments");
        }
    }
}

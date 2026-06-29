using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthSync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Patients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "MedicalRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "InventoryBatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Appointments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "InventoryBatches");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Appointments");
        }
    }
}

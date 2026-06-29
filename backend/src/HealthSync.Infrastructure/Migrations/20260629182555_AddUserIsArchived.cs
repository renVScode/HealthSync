using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthSync.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIsArchived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsArchived",
                table: "Users",
                column: "IsArchived");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_IsArchived",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Users");
        }
    }
}

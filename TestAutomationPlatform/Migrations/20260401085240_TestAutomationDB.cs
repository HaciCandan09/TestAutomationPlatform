using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAutomationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class TestAutomationDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "TestSuites");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Workspaces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TestSuites",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

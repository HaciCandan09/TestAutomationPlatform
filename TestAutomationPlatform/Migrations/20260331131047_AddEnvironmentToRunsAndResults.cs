using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAutomationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddEnvironmentToRunsAndResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Environment",
                table: "RunResults",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Environment",
                table: "RunResults");
        }
    }
}

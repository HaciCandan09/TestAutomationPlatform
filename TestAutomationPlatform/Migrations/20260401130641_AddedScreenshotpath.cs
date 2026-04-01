using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAutomationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddedScreenshotpath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScreenshotPath",
                table: "RunResults",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScreenshotPath",
                table: "RunResults");
        }
    }
}

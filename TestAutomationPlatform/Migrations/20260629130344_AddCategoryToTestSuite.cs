using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAutomationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToTestSuite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "TestSuites",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestSuites_CategoryId",
                table: "TestSuites",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestSuites_Categories_CategoryId",
                table: "TestSuites",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestSuites_Categories_CategoryId",
                table: "TestSuites");

            migrationBuilder.DropIndex(
                name: "IX_TestSuites_CategoryId",
                table: "TestSuites");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "TestSuites");
        }
    }
}

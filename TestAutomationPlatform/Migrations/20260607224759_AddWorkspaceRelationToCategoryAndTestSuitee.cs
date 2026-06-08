using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAutomationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceRelationToCategoryAndTestSuitee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_WorkspaceId",
                table: "Categories",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Workspaces_WorkspaceId",
                table: "Categories",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Workspaces_WorkspaceId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_WorkspaceId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Categories");
        }
    }
}

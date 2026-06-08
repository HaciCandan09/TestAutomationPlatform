using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAutomationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceSuiteCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Scripts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TestSuiteId",
                table: "Scripts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Scripts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestSuites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSuites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestSuites_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_CategoryId",
                table: "Scripts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_TestSuiteId",
                table: "Scripts",
                column: "TestSuiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_WorkspaceId",
                table: "Scripts",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSuites_WorkspaceId",
                table: "TestSuites",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scripts_Categories_CategoryId",
                table: "Scripts",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Scripts_TestSuites_TestSuiteId",
                table: "Scripts",
                column: "TestSuiteId",
                principalTable: "TestSuites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Scripts_Workspaces_WorkspaceId",
                table: "Scripts",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scripts_Categories_CategoryId",
                table: "Scripts");

            migrationBuilder.DropForeignKey(
                name: "FK_Scripts_TestSuites_TestSuiteId",
                table: "Scripts");

            migrationBuilder.DropForeignKey(
                name: "FK_Scripts_Workspaces_WorkspaceId",
                table: "Scripts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "TestSuites");

            migrationBuilder.DropTable(
                name: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Scripts_CategoryId",
                table: "Scripts");

            migrationBuilder.DropIndex(
                name: "IX_Scripts_TestSuiteId",
                table: "Scripts");

            migrationBuilder.DropIndex(
                name: "IX_Scripts_WorkspaceId",
                table: "Scripts");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Scripts");

            migrationBuilder.DropColumn(
                name: "TestSuiteId",
                table: "Scripts");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Scripts");
        }
    }
}

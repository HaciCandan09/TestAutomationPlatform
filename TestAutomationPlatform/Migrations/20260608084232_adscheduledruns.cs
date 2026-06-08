using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAutomationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class adscheduledruns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScriptId = table.Column<int>(type: "int", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CronExpression = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledRuns_Scripts_ScriptId",
                        column: x => x.ScriptId,
                        principalTable: "Scripts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledRuns_ScriptId",
                table: "ScheduledRuns",
                column: "ScriptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledRuns");
        }
    }
}

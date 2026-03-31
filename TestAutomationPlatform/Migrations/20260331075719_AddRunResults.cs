using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestAutomationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddRunResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RunResults_Runs_RunId",
                table: "RunResults");

            migrationBuilder.DropForeignKey(
                name: "FK_RunResults_Scripts_ScriptId",
                table: "RunResults");

            migrationBuilder.DropIndex(
                name: "IX_RunResults_RunId",
                table: "RunResults");

            migrationBuilder.DropIndex(
                name: "IX_RunResults_ScriptId",
                table: "RunResults");

            migrationBuilder.DropColumn(
                name: "RunId",
                table: "RunResults");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExecutedAt",
                table: "RunResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutedAt",
                table: "RunResults");

            migrationBuilder.AddColumn<int>(
                name: "RunId",
                table: "RunResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RunResults_RunId",
                table: "RunResults",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_RunResults_ScriptId",
                table: "RunResults",
                column: "ScriptId");

            migrationBuilder.AddForeignKey(
                name: "FK_RunResults_Runs_RunId",
                table: "RunResults",
                column: "RunId",
                principalTable: "Runs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RunResults_Scripts_ScriptId",
                table: "RunResults",
                column: "ScriptId",
                principalTable: "Scripts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

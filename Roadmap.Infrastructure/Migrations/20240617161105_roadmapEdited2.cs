using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roadmap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class roadmapEdited2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roadmaps_Users_UserId1",
                table: "Roadmaps");

            migrationBuilder.DropForeignKey(
                name: "FK_Roadmaps_Users_UserId2",
                table: "Roadmaps");

            migrationBuilder.DropIndex(
                name: "IX_Roadmaps_UserId1",
                table: "Roadmaps");

            migrationBuilder.DropIndex(
                name: "IX_Roadmaps_UserId2",
                table: "Roadmaps");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Roadmaps");

            migrationBuilder.DropColumn(
                name: "UserId2",
                table: "Roadmaps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Roadmaps",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId2",
                table: "Roadmaps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_UserId1",
                table: "Roadmaps",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_UserId2",
                table: "Roadmaps",
                column: "UserId2");

            migrationBuilder.AddForeignKey(
                name: "FK_Roadmaps_Users_UserId1",
                table: "Roadmaps",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Roadmaps_Users_UserId2",
                table: "Roadmaps",
                column: "UserId2",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

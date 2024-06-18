using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roadmap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class roadmapEdited : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roadmaps_Users_CreatorId",
                table: "Roadmaps");

            migrationBuilder.DropForeignKey(
                name: "FK_Roadmaps_Users_UserId",
                table: "Roadmaps");

            migrationBuilder.DropIndex(
                name: "IX_Roadmaps_CreatorId",
                table: "Roadmaps");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Roadmaps");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Roadmaps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId2",
                table: "Roadmaps",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_UserId2",
                table: "Roadmaps",
                column: "UserId2");

            migrationBuilder.AddForeignKey(
                name: "FK_Roadmaps_Users_UserId",
                table: "Roadmaps",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roadmaps_Users_UserId2",
                table: "Roadmaps",
                column: "UserId2",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roadmaps_Users_UserId",
                table: "Roadmaps");

            migrationBuilder.DropForeignKey(
                name: "FK_Roadmaps_Users_UserId2",
                table: "Roadmaps");

            migrationBuilder.DropIndex(
                name: "IX_Roadmaps_UserId2",
                table: "Roadmaps");

            migrationBuilder.DropColumn(
                name: "UserId2",
                table: "Roadmaps");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Roadmaps",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Roadmaps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_CreatorId",
                table: "Roadmaps",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roadmaps_Users_CreatorId",
                table: "Roadmaps",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roadmaps_Users_UserId",
                table: "Roadmaps",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

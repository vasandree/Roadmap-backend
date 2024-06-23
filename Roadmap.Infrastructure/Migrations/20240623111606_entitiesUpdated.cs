using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roadmap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class entitiesUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaredRoadmaps");

            migrationBuilder.AddColumn<HashSet<Guid>>(
                name: "Stared",
                table: "Users",
                type: "uuid[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StarsCount",
                table: "Roadmaps",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TopicsCount",
                table: "Roadmaps",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stared",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StarsCount",
                table: "Roadmaps");

            migrationBuilder.DropColumn(
                name: "TopicsCount",
                table: "Roadmaps");

            migrationBuilder.CreateTable(
                name: "StaredRoadmaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoadmapId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaredRoadmaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaredRoadmaps_Roadmaps_RoadmapId",
                        column: x => x.RoadmapId,
                        principalTable: "Roadmaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaredRoadmaps_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaredRoadmaps_RoadmapId",
                table: "StaredRoadmaps",
                column: "RoadmapId");

            migrationBuilder.CreateIndex(
                name: "IX_StaredRoadmaps_UserId",
                table: "StaredRoadmaps",
                column: "UserId");
        }
    }
}

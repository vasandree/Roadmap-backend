using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roadmap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class staredRoadmaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaredRoadmaps");
        }
    }
}

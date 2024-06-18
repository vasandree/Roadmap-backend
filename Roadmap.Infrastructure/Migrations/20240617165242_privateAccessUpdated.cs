using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roadmap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class privateAccessUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PrivateAccesses_RoadmapId",
                table: "PrivateAccesses",
                column: "RoadmapId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateAccesses_UserId",
                table: "PrivateAccesses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateAccesses_Roadmaps_RoadmapId",
                table: "PrivateAccesses",
                column: "RoadmapId",
                principalTable: "Roadmaps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateAccesses_Users_UserId",
                table: "PrivateAccesses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrivateAccesses_Roadmaps_RoadmapId",
                table: "PrivateAccesses");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivateAccesses_Users_UserId",
                table: "PrivateAccesses");

            migrationBuilder.DropIndex(
                name: "IX_PrivateAccesses_RoadmapId",
                table: "PrivateAccesses");

            migrationBuilder.DropIndex(
                name: "IX_PrivateAccesses_UserId",
                table: "PrivateAccesses");
        }
    }
}

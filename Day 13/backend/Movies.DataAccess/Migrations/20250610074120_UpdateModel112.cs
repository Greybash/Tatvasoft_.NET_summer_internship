using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movies.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel112 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MissionApps_Missions_MissionId1",
                table: "MissionApps");

            migrationBuilder.DropIndex(
                name: "IX_MissionApps_MissionId1",
                table: "MissionApps");

            migrationBuilder.DropColumn(
                name: "MissionId1",
                table: "MissionApps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MissionId1",
                table: "MissionApps",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MissionApps_MissionId1",
                table: "MissionApps",
                column: "MissionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MissionApps_Missions_MissionId1",
                table: "MissionApps",
                column: "MissionId1",
                principalTable: "Missions",
                principalColumn: "Id");
        }
    }
}

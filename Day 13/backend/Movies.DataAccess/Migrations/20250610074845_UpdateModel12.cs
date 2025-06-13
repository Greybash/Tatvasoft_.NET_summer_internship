using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movies.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MissionApps_Users_UserId1",
                table: "MissionApps");

            migrationBuilder.DropIndex(
                name: "IX_MissionApps_UserId1",
                table: "MissionApps");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "MissionApps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "MissionApps",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MissionApps_UserId1",
                table: "MissionApps",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MissionApps_Users_UserId1",
                table: "MissionApps",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

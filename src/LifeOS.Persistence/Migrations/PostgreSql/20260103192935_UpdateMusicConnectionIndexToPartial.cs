using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeOS.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class UpdateMusicConnectionIndexToPartial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MusicConnections_UserId",
                table: "MusicConnections");

            migrationBuilder.CreateIndex(
                name: "IX_MusicConnections_UserId",
                table: "MusicConnections",
                column: "UserId",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MusicConnections_UserId",
                table: "MusicConnections");

            migrationBuilder.CreateIndex(
                name: "IX_MusicConnections_UserId",
                table: "MusicConnections",
                column: "UserId",
                unique: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeOS.Persistence.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddMusicEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MusicConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SpotifyUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SpotifyUserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SpotifyUserEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConnectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusicConnections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusicListeningHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpotifyTrackId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TrackName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ArtistName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AlbumName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Genre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    ProgressMs = table.Column<int>(type: "integer", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicListeningHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusicListeningHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedTracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpotifyTrackId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Artist = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Album = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    AlbumCoverUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedTracks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicConnections_IsActive",
                table: "MusicConnections",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MusicConnections_SpotifyUserId",
                table: "MusicConnections",
                column: "SpotifyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicConnections_UserId",
                table: "MusicConnections",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusicListeningHistory_PlayedAt",
                table: "MusicListeningHistory",
                column: "PlayedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MusicListeningHistory_SpotifyTrackId",
                table: "MusicListeningHistory",
                column: "SpotifyTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicListeningHistory_UserId",
                table: "MusicListeningHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicListeningHistory_UserId_ArtistName",
                table: "MusicListeningHistory",
                columns: new[] { "UserId", "ArtistName" });

            migrationBuilder.CreateIndex(
                name: "IX_MusicListeningHistory_UserId_PlayedAt",
                table: "MusicListeningHistory",
                columns: new[] { "UserId", "PlayedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SavedTracks_SavedAt",
                table: "SavedTracks",
                column: "SavedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SavedTracks_SpotifyTrackId",
                table: "SavedTracks",
                column: "SpotifyTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedTracks_UserId",
                table: "SavedTracks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedTracks_UserId_SpotifyTrackId",
                table: "SavedTracks",
                columns: new[] { "UserId", "SpotifyTrackId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicConnections");

            migrationBuilder.DropTable(
                name: "MusicListeningHistory");

            migrationBuilder.DropTable(
                name: "SavedTracks");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandaSports.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChannelSports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_channels_sports_sport_id",
                table: "channels");

            migrationBuilder.DropIndex(
                name: "ix_channels_sport_id",
                table: "channels");

            migrationBuilder.DropColumn(
                name: "sport_id",
                table: "channels");

            migrationBuilder.CreateTable(
                name: "channel_sports",
                columns: table => new
                {
                    channel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sports_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channel_sports", x => new { x.channel_id, x.sports_id });
                    table.ForeignKey(
                        name: "fk_channel_sports_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_channel_sports_sports_sports_id",
                        column: x => x.sports_id,
                        principalTable: "sports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_channel_sports_sports_id",
                table: "channel_sports",
                column: "sports_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "channel_sports");

            migrationBuilder.AddColumn<Guid>(
                name: "sport_id",
                table: "channels",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_channels_sport_id",
                table: "channels",
                column: "sport_id");

            migrationBuilder.AddForeignKey(
                name: "fk_channels_sports_sport_id",
                table: "channels",
                column: "sport_id",
                principalTable: "sports",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandaSports.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChannelAndOpinionSport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "sport_id",
                table: "opinions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "sport_id",
                table: "channels",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_opinions_sport_id",
                table: "opinions",
                column: "sport_id");

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

            migrationBuilder.AddForeignKey(
                name: "fk_opinions_sports_sport_id",
                table: "opinions",
                column: "sport_id",
                principalTable: "sports",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_channels_sports_sport_id",
                table: "channels");

            migrationBuilder.DropForeignKey(
                name: "fk_opinions_sports_sport_id",
                table: "opinions");

            migrationBuilder.DropIndex(
                name: "ix_opinions_sport_id",
                table: "opinions");

            migrationBuilder.DropIndex(
                name: "ix_channels_sport_id",
                table: "channels");

            migrationBuilder.DropColumn(
                name: "sport_id",
                table: "opinions");

            migrationBuilder.DropColumn(
                name: "sport_id",
                table: "channels");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandaSports.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentaryPipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "story_id",
                table: "opinions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "you_tube_channel_id",
                table: "channels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "handle",
                table: "channels",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "last_error",
                table: "channels",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_opinions_story_id_status",
                table: "opinions",
                columns: new[] { "story_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_channels_handle",
                table: "channels",
                column: "handle",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_opinions_stories_story_id",
                table: "opinions",
                column: "story_id",
                principalTable: "stories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_opinions_stories_story_id",
                table: "opinions");

            migrationBuilder.DropIndex(
                name: "ix_opinions_story_id_status",
                table: "opinions");

            migrationBuilder.DropIndex(
                name: "ix_channels_handle",
                table: "channels");

            migrationBuilder.DropColumn(
                name: "story_id",
                table: "opinions");

            migrationBuilder.DropColumn(
                name: "handle",
                table: "channels");

            migrationBuilder.DropColumn(
                name: "last_error",
                table: "channels");

            migrationBuilder.AlterColumn<string>(
                name: "you_tube_channel_id",
                table: "channels",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}

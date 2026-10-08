using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandaSports.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StoryTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "story_teams",
                columns: table => new
                {
                    story_id = table.Column<Guid>(type: "uuid", nullable: false),
                    teams_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_story_teams", x => new { x.story_id, x.teams_id });
                    table.ForeignKey(
                        name: "fk_story_teams_stories_story_id",
                        column: x => x.story_id,
                        principalTable: "stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_story_teams_teams_teams_id",
                        column: x => x.teams_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_story_teams_teams_id",
                table: "story_teams",
                column: "teams_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "story_teams");
        }
    }
}

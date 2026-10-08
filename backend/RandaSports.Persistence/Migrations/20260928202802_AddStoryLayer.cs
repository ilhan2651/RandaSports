using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandaSports.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoryLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "priority",
                table: "sources",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "story_id",
                table: "articles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "stories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sport_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cluster_key = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    slug = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    headline = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    body = table.Column<string>(type: "text", nullable: true),
                    analysis = table.Column<string>(type: "text", nullable: true),
                    is_live = table.Column<bool>(type: "boolean", nullable: false),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    image_priority = table.Column<int>(type: "integer", nullable: false),
                    first_published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    source_count = table.Column<int>(type: "integer", nullable: false),
                    needs_rewrite = table.Column<bool>(type: "boolean", nullable: false),
                    ai_processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ai_attempts = table.Column<int>(type: "integer", nullable: false),
                    ai_data_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stories", x => x.id);
                    table.ForeignKey(
                        name: "fk_stories_sports_sport_id",
                        column: x => x.sport_id,
                        principalTable: "sports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_articles_story_id",
                table: "articles",
                column: "story_id",
                filter: "story_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_stories_cluster_key",
                table: "stories",
                column: "cluster_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stories_last_published_at",
                table: "stories",
                column: "last_published_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_stories_needs_rewrite",
                table: "stories",
                column: "needs_rewrite",
                filter: "needs_rewrite = true");

            migrationBuilder.CreateIndex(
                name: "ix_stories_slug",
                table: "stories",
                column: "slug",
                unique: true,
                filter: "slug IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_stories_sport_id_last_published_at",
                table: "stories",
                columns: new[] { "sport_id", "last_published_at" },
                descending: new[] { false, true });

            migrationBuilder.AddForeignKey(
                name: "fk_articles_stories_story_id",
                table: "articles",
                column: "story_id",
                principalTable: "stories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_articles_stories_story_id",
                table: "articles");

            migrationBuilder.DropTable(
                name: "stories");

            migrationBuilder.DropIndex(
                name: "ix_articles_story_id",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "sources");

            migrationBuilder.DropColumn(
                name: "story_id",
                table: "articles");
        }
    }
}

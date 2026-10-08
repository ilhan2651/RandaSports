using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandaSports.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleAiContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ai_body",
                table: "articles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ai_headline",
                table: "articles",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ai_is_live",
                table: "articles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ai_body",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "ai_headline",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "ai_is_live",
                table: "articles");
        }
    }
}

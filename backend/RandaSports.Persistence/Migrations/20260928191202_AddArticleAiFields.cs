using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandaSports.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleAiFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ai_attempts",
                table: "articles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ai_data_json",
                table: "articles",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ai_attempts",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "ai_data_json",
                table: "articles");
        }
    }
}

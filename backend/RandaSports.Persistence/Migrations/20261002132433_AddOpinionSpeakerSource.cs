using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandaSports.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOpinionSpeakerSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "speaker_source",
                table: "opinions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "speaker_source",
                table: "opinions");
        }
    }
}

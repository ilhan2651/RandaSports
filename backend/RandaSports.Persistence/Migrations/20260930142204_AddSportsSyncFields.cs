using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandaSports.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSportsSyncFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "squad_synced_at",
                table: "teams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "appearances",
                table: "athletes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "assists",
                table: "athletes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "birth_place",
                table: "athletes",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "goals",
                table: "athletes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "height_cm",
                table: "athletes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "minutes_played",
                table: "athletes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "rating",
                table: "athletes",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "shirt_number",
                table: "athletes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stats_json",
                table: "athletes",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "stats_season",
                table: "athletes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "stats_updated_at",
                table: "athletes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "weight_kg",
                table: "athletes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "provider_quotas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    used = table.Column<int>(type: "integer", nullable: false),
                    daily_limit = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_quotas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_provider_quotas_provider_day",
                table: "provider_quotas",
                columns: new[] { "provider", "day" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "provider_quotas");

            migrationBuilder.DropColumn(
                name: "squad_synced_at",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "appearances",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "assists",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "birth_place",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "goals",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "height_cm",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "minutes_played",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "rating",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "shirt_number",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "stats_json",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "stats_season",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "stats_updated_at",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "weight_kg",
                table: "athletes");
        }
    }
}

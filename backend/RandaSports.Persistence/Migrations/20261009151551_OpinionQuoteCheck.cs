using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandaSports.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OpinionQuoteCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "quote_check",
                table: "opinions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "quote_check_heard",
                table: "opinions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "quote_check_speaker",
                table: "opinions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "quote_check_timestamp_seconds",
                table: "opinions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "quote_checked_at",
                table: "opinions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_opinions_status_quote_check",
                table: "opinions",
                columns: new[] { "status", "quote_check" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_opinions_status_quote_check",
                table: "opinions");

            migrationBuilder.DropColumn(
                name: "quote_check",
                table: "opinions");

            migrationBuilder.DropColumn(
                name: "quote_check_heard",
                table: "opinions");

            migrationBuilder.DropColumn(
                name: "quote_check_speaker",
                table: "opinions");

            migrationBuilder.DropColumn(
                name: "quote_check_timestamp_seconds",
                table: "opinions");

            migrationBuilder.DropColumn(
                name: "quote_checked_at",
                table: "opinions");
        }
    }
}

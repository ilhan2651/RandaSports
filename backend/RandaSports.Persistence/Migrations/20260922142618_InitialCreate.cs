using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandaSports.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    you_tube_channel_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_checked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "commentators",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    aliases = table.Column<List<string>>(type: "text[]", nullable: false),
                    bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_commentators", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "videos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    you_tube_video_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    thumbnail_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    processing_status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    processing_attempts = table.Column<int>(type: "integer", nullable: false),
                    processing_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ai_summary = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_videos", x => x.id);
                    table.ForeignKey(
                        name: "fk_videos_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "channel_commentators",
                columns: table => new
                {
                    channels_id = table.Column<Guid>(type: "uuid", nullable: false),
                    regular_commentators_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channel_commentators", x => new { x.channels_id, x.regular_commentators_id });
                    table.ForeignKey(
                        name: "fk_channel_commentators_channels_channels_id",
                        column: x => x.channels_id,
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_channel_commentators_commentators_regular_commentators_id",
                        column: x => x.regular_commentators_id,
                        principalTable: "commentators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "commentator_sports",
                columns: table => new
                {
                    commentator_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sports_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_commentator_sports", x => new { x.commentator_id, x.sports_id });
                    table.ForeignKey(
                        name: "fk_commentator_sports_commentators_commentator_id",
                        column: x => x.commentator_id,
                        principalTable: "commentators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_commentator_sports_sports_sports_id",
                        column: x => x.sports_id,
                        principalTable: "sports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "competitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sport_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    external_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_competitions", x => x.id);
                    table.ForeignKey(
                        name: "fk_competitions_sports_sport_id",
                        column: x => x.sport_id,
                        principalTable: "sports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    sport_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    fetch_interval_minutes = table.Column<int>(type: "integer", nullable: false),
                    last_fetched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sources", x => x.id);
                    table.ForeignKey(
                        name: "fk_sources_sports_sport_id",
                        column: x => x.sport_id,
                        principalTable: "sports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sport_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    short_name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_national = table.Column<bool>(type: "boolean", nullable: false),
                    external_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teams", x => x.id);
                    table.ForeignKey(
                        name: "fk_teams_sports_sport_id",
                        column: x => x.sport_id,
                        principalTable: "sports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "seasons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    competition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    external_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_seasons", x => x.id);
                    table.ForeignKey(
                        name: "fk_seasons_competitions_competition_id",
                        column: x => x.competition_id,
                        principalTable: "competitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "articles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sport_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    excerpt = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    author = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ai_summary = table.Column<string>(type: "text", nullable: true),
                    ai_analysis = table.Column<string>(type: "text", nullable: true),
                    ai_processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_articles", x => x.id);
                    table.ForeignKey(
                        name: "fk_articles_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_articles_sports_sport_id",
                        column: x => x.sport_id,
                        principalTable: "sports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "athletes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sport_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: true),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    external_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_athletes", x => x.id);
                    table.ForeignKey(
                        name: "fk_athletes_sports_sport_id",
                        column: x => x.sport_id,
                        principalTable: "sports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_athletes_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "competition_teams",
                columns: table => new
                {
                    competitions_id = table.Column<Guid>(type: "uuid", nullable: false),
                    teams_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_competition_teams", x => new { x.competitions_id, x.teams_id });
                    table.ForeignKey(
                        name: "fk_competition_teams_competitions_competitions_id",
                        column: x => x.competitions_id,
                        principalTable: "competitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_competition_teams_teams_teams_id",
                        column: x => x.teams_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    start_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status_detail = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    round = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    venue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    stats_json = table.Column<string>(type: "jsonb", nullable: true),
                    external_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_events_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "article_tags",
                columns: table => new
                {
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_tags", x => new { x.article_id, x.entity_type, x.entity_id });
                    table.ForeignKey(
                        name: "fk_article_tags_articles_article_id",
                        column: x => x.article_id,
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "standings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: true),
                    athlete_id = table.Column<Guid>(type: "uuid", nullable: true),
                    group_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    played = table.Column<int>(type: "integer", nullable: false),
                    won = table.Column<int>(type: "integer", nullable: false),
                    drawn = table.Column<int>(type: "integer", nullable: false),
                    lost = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: true),
                    form = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    stats_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_standings", x => x.id);
                    table.CheckConstraint("ck_standings_team_or_athlete", "team_id IS NOT NULL OR athlete_id IS NOT NULL");
                    table.ForeignKey(
                        name: "fk_standings_athletes_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athletes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_standings_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_standings_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "event_participants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: true),
                    athlete_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: true),
                    score_detail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    stats_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_participants", x => x.id);
                    table.CheckConstraint("ck_event_participants_team_or_athlete", "team_id IS NOT NULL OR athlete_id IS NOT NULL");
                    table.ForeignKey(
                        name: "fk_event_participants_athletes_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athletes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_event_participants_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_event_participants_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "opinions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    commentator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    speaker_label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    attribution_confidence = table.Column<double>(type: "double precision", nullable: true),
                    event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    team_id = table.Column<Guid>(type: "uuid", nullable: true),
                    athlete_id = table.Column<Guid>(type: "uuid", nullable: true),
                    topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    quote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    timestamp_seconds = table.Column<int>(type: "integer", nullable: false),
                    stance = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    prediction = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    is_quote_verified = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    review_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_opinions", x => x.id);
                    table.CheckConstraint("ck_opinions_attribution_confidence", "attribution_confidence IS NULL OR (attribution_confidence >= 0 AND attribution_confidence <= 1)");
                    table.ForeignKey(
                        name: "fk_opinions_athletes_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athletes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_opinions_commentators_commentator_id",
                        column: x => x.commentator_id,
                        principalTable: "commentators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_opinions_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_opinions_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_opinions_videos_video_id",
                        column: x => x.video_id,
                        principalTable: "videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_article_tags_entity_type_entity_id",
                table: "article_tags",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_articles_ai_processed_at",
                table: "articles",
                column: "ai_processed_at",
                filter: "ai_processed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_articles_published_at",
                table: "articles",
                column: "published_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_articles_source_id",
                table: "articles",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "ix_articles_sport_id_published_at",
                table: "articles",
                columns: new[] { "sport_id", "published_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_articles_url",
                table: "articles",
                column: "url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_athletes_external_id",
                table: "athletes",
                column: "external_id");

            migrationBuilder.CreateIndex(
                name: "ix_athletes_sport_id_slug",
                table: "athletes",
                columns: new[] { "sport_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_athletes_team_id",
                table: "athletes",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_channel_commentators_regular_commentators_id",
                table: "channel_commentators",
                column: "regular_commentators_id");

            migrationBuilder.CreateIndex(
                name: "ix_channels_slug",
                table: "channels",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_channels_you_tube_channel_id",
                table: "channels",
                column: "you_tube_channel_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_commentator_sports_sports_id",
                table: "commentator_sports",
                column: "sports_id");

            migrationBuilder.CreateIndex(
                name: "ix_commentators_slug",
                table: "commentators",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_competition_teams_teams_id",
                table: "competition_teams",
                column: "teams_id");

            migrationBuilder.CreateIndex(
                name: "ix_competitions_external_id",
                table: "competitions",
                column: "external_id");

            migrationBuilder.CreateIndex(
                name: "ix_competitions_sport_id_slug",
                table: "competitions",
                columns: new[] { "sport_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_participants_athlete_id",
                table: "event_participants",
                column: "athlete_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_participants_event_id_order",
                table: "event_participants",
                columns: new[] { "event_id", "order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_participants_team_id",
                table: "event_participants",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_external_id",
                table: "events",
                column: "external_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_season_id_start_time",
                table: "events",
                columns: new[] { "season_id", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_events_slug",
                table: "events",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_events_status_start_time",
                table: "events",
                columns: new[] { "status", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_opinions_athlete_id_status",
                table: "opinions",
                columns: new[] { "athlete_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_opinions_commentator_id_status",
                table: "opinions",
                columns: new[] { "commentator_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_opinions_event_id_status",
                table: "opinions",
                columns: new[] { "event_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_opinions_status_created_at",
                table: "opinions",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_opinions_team_id_status",
                table: "opinions",
                columns: new[] { "team_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_opinions_video_id",
                table: "opinions",
                column: "video_id");

            migrationBuilder.CreateIndex(
                name: "ix_seasons_competition_id",
                table: "seasons",
                column: "competition_id",
                unique: true,
                filter: "is_current = true");

            migrationBuilder.CreateIndex(
                name: "ix_seasons_competition_id_name",
                table: "seasons",
                columns: new[] { "competition_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sources_sport_id",
                table: "sources",
                column: "sport_id");

            migrationBuilder.CreateIndex(
                name: "ix_sources_url",
                table: "sources",
                column: "url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sports_slug",
                table: "sports",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_standings_athlete_id",
                table: "standings",
                column: "athlete_id");

            migrationBuilder.CreateIndex(
                name: "ix_standings_season_id_group_name_rank",
                table: "standings",
                columns: new[] { "season_id", "group_name", "rank" });

            migrationBuilder.CreateIndex(
                name: "ix_standings_team_id",
                table: "standings",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_teams_external_id",
                table: "teams",
                column: "external_id");

            migrationBuilder.CreateIndex(
                name: "ix_teams_sport_id_slug",
                table: "teams",
                columns: new[] { "sport_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_videos_channel_id_published_at",
                table: "videos",
                columns: new[] { "channel_id", "published_at" });

            migrationBuilder.CreateIndex(
                name: "ix_videos_processing_status_published_at",
                table: "videos",
                columns: new[] { "processing_status", "published_at" });

            migrationBuilder.CreateIndex(
                name: "ix_videos_you_tube_video_id",
                table: "videos",
                column: "you_tube_video_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_tags");

            migrationBuilder.DropTable(
                name: "channel_commentators");

            migrationBuilder.DropTable(
                name: "commentator_sports");

            migrationBuilder.DropTable(
                name: "competition_teams");

            migrationBuilder.DropTable(
                name: "event_participants");

            migrationBuilder.DropTable(
                name: "opinions");

            migrationBuilder.DropTable(
                name: "standings");

            migrationBuilder.DropTable(
                name: "articles");

            migrationBuilder.DropTable(
                name: "commentators");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "videos");

            migrationBuilder.DropTable(
                name: "athletes");

            migrationBuilder.DropTable(
                name: "sources");

            migrationBuilder.DropTable(
                name: "seasons");

            migrationBuilder.DropTable(
                name: "channels");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "competitions");

            migrationBuilder.DropTable(
                name: "sports");
        }
    }
}

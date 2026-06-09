using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PreviewImage = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SceneData = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Visibility = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Difficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quests_Categories",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Quests_Users",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RegistrationTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationTokens_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateRenames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SystemKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateRenames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateRenames_RoomTemplates",
                        column: x => x.TemplateId,
                        principalTable: "RoomTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TemplateRenames_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    QuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestRooms_Quests",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestRooms_Templates",
                        column: x => x.RoomTemplateId,
                        principalTable: "RoomTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuestSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    QuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeLimit = table.Column<int>(type: "integer", nullable: true),
                    AllowPartialCompletion = table.Column<bool>(type: "boolean", nullable: false),
                    AllowToSkip = table.Column<bool>(type: "boolean", nullable: false),
                    AccessCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestSessions_Quests",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestSessions_Users",
                        column: x => x.StartedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    QuestRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetObject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Attachment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Hint = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_QuestRooms",
                        column: x => x.QuestRoomId,
                        principalTable: "QuestRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attempts_Sessions",
                        column: x => x.QuestSessionId,
                        principalTable: "QuestSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attempts_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnswerOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: true),
                    MatchPair = table.Column<string>(type: "text", nullable: true),
                    SequenceOrder = table.Column<int>(type: "integer", nullable: true),
                    Attachment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerOptions_Questions",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerData = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    PointsAwarded = table.Column<int>(type: "integer", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAnswers_Attempts",
                        column: x => x.AttemptId,
                        principalTable: "Attempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAnswers_Questions",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionId",
                table: "AnswerOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_QuestSessionId",
                table: "Attempts",
                column: "QuestSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_UserId",
                table: "Attempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuestRoomId",
                table: "Questions",
                column: "QuestRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestRooms_QuestId",
                table: "QuestRooms",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestRooms_RoomTemplateId",
                table: "QuestRooms",
                column: "RoomTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_AuthorId",
                table: "Quests",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_CategoryId",
                table: "Quests",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestSessions_QuestId",
                table: "QuestSessions",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestSessions_StartedBy",
                table: "QuestSessions",
                column: "StartedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__QuestSes__24C20D0C80A32EF2",
                table: "QuestSessions",
                column: "AccessCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationTokens_UserId",
                table: "RegistrationTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Registra__9A2B249A5C9F217E",
                table: "RegistrationTokens",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateRenames_TemplateId",
                table: "TemplateRenames",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateRenames_UserId",
                table: "TemplateRenames",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_AttemptId",
                table: "UserAnswers",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuestionId",
                table: "UserAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__536C85E454869B91",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerOptions");

            migrationBuilder.DropTable(
                name: "RegistrationTokens");

            migrationBuilder.DropTable(
                name: "TemplateRenames");

            migrationBuilder.DropTable(
                name: "UserAnswers");

            migrationBuilder.DropTable(
                name: "Attempts");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "QuestSessions");

            migrationBuilder.DropTable(
                name: "QuestRooms");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropTable(
                name: "RoomTemplates");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

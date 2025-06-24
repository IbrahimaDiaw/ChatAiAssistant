using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatAI_Assistant.Server.Migrations
{
    /// <inheritdoc />
    public partial class ADD_DATABASES_SCHEMA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastActivity = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TotalMessages = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalSessions = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastActivity = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MessageCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ParticipantCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SessionAIProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SessionAIModel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SessionContext = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatSessions_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreferredAIProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "OpenAI"),
                    PreferredModel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Temperature = table.Column<double>(type: "float(3)", precision: 3, scale: 2, nullable: false, defaultValue: 0.69999999999999996),
                    MaxTokens = table.Column<int>(type: "int", nullable: false, defaultValue: 1000),
                    SystemPrompt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Theme = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "light"),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "fr"),
                    EnableNotifications = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EnableSoundEffects = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ShowTypingIndicator = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.CheckConstraint("CK_UserPreferences_MaxTokens", "[MaxTokens] >= 1 AND [MaxTokens] <= 4000");
                    table.CheckConstraint("CK_UserPreferences_Temperature", "[Temperature] >= 0.0 AND [Temperature] <= 2.0");
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "User"),
                    IsFromAI = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ParentMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MessageHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AIProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AIModel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TokensUsed = table.Column<int>(type: "int", nullable: true),
                    AITemperature = table.Column<double>(type: "float(3)", precision: 3, scale: 2, nullable: true),
                    AIContext = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.CheckConstraint("CK_ChatMessages_AITemperature", "[AITemperature] IS NULL OR ([AITemperature] >= 0.0 AND [AITemperature] <= 2.0)");
                    table.CheckConstraint("CK_ChatMessages_TokensUsed", "[TokensUsed] IS NULL OR [TokensUsed] > 0");
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatMessages_ParentMessageId",
                        column: x => x.ParentMessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsModerator = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "participant"),
                    MessagesCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionParticipants", x => x.Id);
                    table.CheckConstraint("CK_SessionParticipants_Role", "[Role] IN ('participant', 'moderator', 'admin')");
                    table.ForeignKey(
                        name: "FK_SessionParticipants_ChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "DisplayName", "IsActive", "LastActivity", "Username" },
                values: new object[] { new Guid("2f1bff77-fc98-410e-b3ec-0cecb5021733"), null, new DateTime(2025, 6, 24, 11, 14, 30, 313, DateTimeKind.Utc).AddTicks(3615), "Test User", true, new DateTime(2025, 6, 24, 11, 14, 30, 313, DateTimeKind.Utc).AddTicks(3616), "testuser" });

            migrationBuilder.InsertData(
                table: "ChatSessions",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "Description", "IsActive", "LastActivity", "ParticipantCount", "SessionAIModel", "SessionAIProvider", "SessionContext", "Title" },
                values: new object[] { new Guid("434ecdd6-0ecf-4e81-a819-4d10161c3222"), new DateTime(2025, 6, 24, 11, 14, 30, 313, DateTimeKind.Utc).AddTicks(3906), new Guid("2f1bff77-fc98-410e-b3ec-0cecb5021733"), "Session de test pour développement", true, new DateTime(2025, 6, 24, 11, 14, 30, 313, DateTimeKind.Utc).AddTicks(3907), 1, null, null, null, "Session de Test" });

            migrationBuilder.InsertData(
                table: "UserPreferences",
                columns: new[] { "Id", "EnableNotifications", "EnableSoundEffects", "Language", "MaxTokens", "PreferredAIProvider", "PreferredModel", "ShowTypingIndicator", "SystemPrompt", "Temperature", "Theme", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("0bbd7a42-c05f-43ab-bed0-10f6f440d60b"), true, true, "fr", 1000, "OpenAI", null, true, null, 0.69999999999999996, "light", new DateTime(2025, 6, 24, 11, 14, 30, 313, DateTimeKind.Utc).AddTicks(3867), new Guid("2f1bff77-fc98-410e-b3ec-0cecb5021733") });

            migrationBuilder.InsertData(
                table: "SessionParticipants",
                columns: new[] { "Id", "IsActive", "JoinedAt", "LastSeenAt", "LeftAt", "Role", "SessionId", "UserId" },
                values: new object[] { new Guid("f5a07917-7b79-4f20-8791-b4b723cc6690"), true, new DateTime(2025, 6, 24, 11, 14, 30, 313, DateTimeKind.Utc).AddTicks(3948), new DateTime(2025, 6, 24, 11, 14, 30, 313, DateTimeKind.Utc).AddTicks(3949), null, "admin", new Guid("434ecdd6-0ecf-4e81-a819-4d10161c3222"), new Guid("2f1bff77-fc98-410e-b3ec-0cecb5021733") });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_AI_Provider",
                table: "ChatMessages",
                columns: new[] { "IsFromAI", "AIProvider" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_MessageHash",
                table: "ChatMessages",
                column: "MessageHash");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ParentMessageId",
                table: "ChatMessages",
                column: "ParentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_Session_NotDeleted_Timestamp",
                table: "ChatMessages",
                columns: new[] { "SessionId", "IsDeleted", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_Session_Timestamp",
                table: "ChatMessages",
                columns: new[] { "SessionId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_Timestamp",
                table: "ChatMessages",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_User_Timestamp",
                table: "ChatMessages",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_Active_LastActivity",
                table: "ChatSessions",
                columns: new[] { "IsActive", "LastActivity" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_CreatedByUserId",
                table: "ChatSessions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_LastActivity",
                table: "ChatSessions",
                column: "LastActivity");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_User_Active_LastActivity",
                table: "ChatSessions",
                columns: new[] { "CreatedByUserId", "IsActive", "LastActivity" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_LastSeenAt",
                table: "SessionParticipants",
                column: "LastSeenAt");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_Session_Active",
                table: "SessionParticipants",
                columns: new[] { "SessionId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_Session_User",
                table: "SessionParticipants",
                columns: new[] { "SessionId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_User_Active",
                table: "SessionParticipants",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_AIProvider",
                table: "UserPreferences",
                column: "PreferredAIProvider");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Active_LastActivity",
                table: "Users",
                columns: new[] { "IsActive", "LastActivity" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastActivity",
                table: "Users",
                column: "LastActivity");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "SessionParticipants");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "ChatSessions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

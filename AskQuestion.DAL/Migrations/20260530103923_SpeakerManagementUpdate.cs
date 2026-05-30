using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SpeakerManagementUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "UserDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "UserDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserDetails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Patronymic",
                table: "UserDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "UserDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SpeakerId",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                table: "UserDetails",
                columns: new[] { "Id", "AdditionalInfo", "Сreated", "Email", "FirstName", "IsDeleted", "LastName", "Patronymic", "Position", "Updated", "UserId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), null, new DateTimeOffset(new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "admin@askquestion.local", "Admin", false, "Admin", null, null, null, new Guid("00000000-0000-0000-0000-000000000001") });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SpeakerId",
                table: "Questions",
                column: "SpeakerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_SpeakerId",
                table: "Questions",
                column: "SpeakerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql(@"
                UPDATE ""UserDetails""
                SET ""FirstName"" = split_part(""FullName"", ' ', 1),
                    ""LastName"" = CASE 
                        WHEN array_length(string_to_array(""FullName"", ' '), 1) = 1 THEN split_part(""FullName"", ' ', 1)
                        WHEN array_length(string_to_array(""FullName"", ' '), 1) = 2 THEN split_part(""FullName"", ' ', 2)
                        ELSE split_part(""FullName"", ' ', 2)
                    END,
                    ""Patronymic"" = CASE 
                        WHEN array_length(string_to_array(""FullName"", ' '), 1) >= 3 THEN split_part(""FullName"", ' ', 3)
                        ELSE NULL
                    END
                WHERE ""FullName"" IS NOT NULL
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Questions""
                SET ""SpeakerId"" = u.""Id""
                FROM ""Users"" u
                INNER JOIN ""UserDetails"" ud ON u.""Id"" = ud.""UserId""
                WHERE ""Questions"".""Speaker"" = ud.""FullName""
                  AND u.""UserRoleId"" = 2
            ");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "Speaker",
                table: "Questions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_SpeakerId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_SpeakerId",
                table: "Questions");

            migrationBuilder.DeleteData(
                table: "UserDetails",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "UserDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE ""UserDetails""
                SET ""FullName"" = CASE
                    WHEN ""Patronymic"" IS NOT NULL THEN ""FirstName"" || ' ' || ""LastName"" || ' ' || ""Patronymic""
                    ELSE ""FirstName"" || ' ' || ""LastName""
                END
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Questions""
                SET ""Speaker"" = ud.""FullName""
                FROM ""UserDetails"" ud
                INNER JOIN ""Users"" u ON u.""Id"" = ud.""UserId""
                WHERE ""Questions"".""SpeakerId"" = u.""Id""
                  AND u.""UserRoleId"" = 2
            ");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "Patronymic",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "SpeakerId",
                table: "Questions");
        }
    }
}

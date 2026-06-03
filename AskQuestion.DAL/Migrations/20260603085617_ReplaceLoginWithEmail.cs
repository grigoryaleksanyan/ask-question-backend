using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceLoginWithEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                @"UPDATE ""Users"" SET ""Email"" = ""UserDetails"".""Email""
                  FROM ""UserDetails""
                  WHERE ""Users"".""Id"" = ""UserDetails"".""UserId""");

            migrationBuilder.Sql(
                @"DO $$
                  BEGIN
                    IF EXISTS (
                      SELECT ""Email"", COUNT(*)
                      FROM ""Users""
                      GROUP BY ""Email""
                      HAVING COUNT(*) > 1
                    ) THEN
                      RAISE EXCEPTION 'Duplicate emails found in Users table. Cannot create unique index.';
                    END IF;
                  END
                  $$");

            migrationBuilder.DropIndex(
                name: "IX_Users_Login",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.DropColumn(
                name: "Login",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "UserDetails");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "Email",
                value: "admin@askquestion.local");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "UserDetails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                @"UPDATE ""UserDetails"" SET ""Email"" = ""Users"".""Email""
                  FROM ""Users""
                  WHERE ""UserDetails"".""UserId"" = ""Users"".""Id""");

            migrationBuilder.Sql(
                @"UPDATE ""Users"" SET ""Login"" = split_part(""Email"", '@', 1)");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                table: "Users",
                column: "Login",
                unique: true);

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "UserDetails",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "Email",
                value: "admin@askquestion.local");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "Login",
                value: "Admin");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeedWithStaticValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("262c937a-fe40-45de-88a9-4fafee4aae1e"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Сreated", "Login", "Password", "Updated", "UserRoleId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Admin", "$2a$11$q385hN2923xQ0sWnC9I84eaC4jNqSx8m9HzZgZYUxBjh9vBX8cr1S", null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Сreated", "Login", "Password", "Updated", "UserRoleId" },
                values: new object[] { new Guid("262c937a-fe40-45de-88a9-4fafee4aae1e"), new DateTimeOffset(new DateTime(2026, 5, 29, 16, 27, 2, 649, DateTimeKind.Unspecified).AddTicks(4141), new TimeSpan(0, 0, 0, 0, 0)), "Admin", "$2a$11$xU290PNC5AhsmkUzR.3BpeKREsMJSm6uqsurKQFYl5zKU3DKqznc2", null, 1 });
        }
    }
}

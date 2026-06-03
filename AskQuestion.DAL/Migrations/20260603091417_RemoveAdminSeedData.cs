using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdminSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserDetails",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Created", "Email", "Password", "Updated", "UserRoleId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "admin@askquestion.local", "$2a$11$q385hN2923xQ0sWnC9I84eaC4jNqSx8m9HzZgZYUxBjh9vBX8cr1S", null, 1 });

            migrationBuilder.InsertData(
                table: "UserDetails",
                columns: new[] { "Id", "AdditionalInfo", "Created", "FirstName", "IsDeleted", "LastName", "Patronymic", "Position", "Updated", "UserId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), null, new DateTimeOffset(new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Admin", false, "Admin", null, null, null, new Guid("00000000-0000-0000-0000-000000000001") });
        }
    }
}

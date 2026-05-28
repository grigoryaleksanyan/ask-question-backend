using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixCyrillicCreatedProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("64f2b7ef-5a3d-4b9f-a5f3-2b38776ecaf4"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Сreated", "Login", "Password", "Updated", "UserRoleId" },
                values: new object[] { new Guid("0bedc959-e585-46df-b197-8a6f499ffabc"), new DateTimeOffset(new DateTime(2026, 5, 28, 12, 14, 6, 15, DateTimeKind.Unspecified).AddTicks(2617), new TimeSpan(0, 0, 0, 0, 0)), "Admin", "$2a$11$iiSAbIFe.mANGjFOxuXf.evse1E6YK0z3IWw78Q8oS6w3dEJ2FZfq", null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0bedc959-e585-46df-b197-8a6f499ffabc"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Login", "Password", "Updated", "UserRoleId", "Сreated" },
                values: new object[] { new Guid("64f2b7ef-5a3d-4b9f-a5f3-2b38776ecaf4"), "Admin", "$2a$11$MFy029zl31FZKB0uHT6jx.Uc7Q3TEIZjLKPi9utOY7QF4g3McQF7G", null, 1, new DateTimeOffset(new DateTime(2023, 4, 10, 14, 16, 41, 146, DateTimeKind.Unspecified).AddTicks(269), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}

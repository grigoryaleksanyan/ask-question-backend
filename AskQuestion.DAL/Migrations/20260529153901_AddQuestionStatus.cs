using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0bedc959-e585-46df-b197-8a6f499ffabc"));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE \"Questions\" SET \"Status\" = 3 WHERE \"Answered\" IS NOT NULL");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Сreated", "Login", "Password", "Updated", "UserRoleId" },
                values: new object[] { new Guid("0dc7617a-9839-4a71-b719-1bb18ccf2b36"), new DateTimeOffset(new DateTime(2026, 5, 29, 15, 38, 59, 815, DateTimeKind.Unspecified).AddTicks(9264), new TimeSpan(0, 0, 0, 0, 0)), "Admin", "$2a$11$CWS64rpb86JyrBIj6T/Ok./nWtY.3LwP70jxY5Cr6KJ4WqNmtJadi", null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0dc7617a-9839-4a71-b719-1bb18ccf2b36"));

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Questions");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Сreated", "Login", "Password", "Updated", "UserRoleId" },
                values: new object[] { new Guid("0bedc959-e585-46df-b197-8a6f499ffabc"), new DateTimeOffset(new DateTime(2026, 5, 28, 12, 14, 6, 15, DateTimeKind.Unspecified).AddTicks(2617), new TimeSpan(0, 0, 0, 0, 0)), "Admin", "$2a$11$iiSAbIFe.mANGjFOxuXf.evse1E6YK0z3IWw78Q8oS6w3dEJ2FZfq", null, 1 });
        }
    }
}

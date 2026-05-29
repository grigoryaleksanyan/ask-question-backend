using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0dc7617a-9839-4a71-b719-1bb18ccf2b36"));

            migrationBuilder.CreateTable(
                name: "QuestionVotes",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoteType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionVotes", x => new { x.QuestionId, x.VisitorId });
                    table.ForeignKey(
                        name: "FK_QuestionVotes_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Сreated", "Login", "Password", "Updated", "UserRoleId" },
                values: new object[] { new Guid("262c937a-fe40-45de-88a9-4fafee4aae1e"), new DateTimeOffset(new DateTime(2026, 5, 29, 16, 27, 2, 649, DateTimeKind.Unspecified).AddTicks(4141), new TimeSpan(0, 0, 0, 0, 0)), "Admin", "$2a$11$xU290PNC5AhsmkUzR.3BpeKREsMJSm6uqsurKQFYl5zKU3DKqznc2", null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionVotes");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("262c937a-fe40-45de-88a9-4fafee4aae1e"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Сreated", "Login", "Password", "Updated", "UserRoleId" },
                values: new object[] { new Guid("0dc7617a-9839-4a71-b719-1bb18ccf2b36"), new DateTimeOffset(new DateTime(2026, 5, 29, 15, 38, 59, 815, DateTimeKind.Unspecified).AddTicks(9264), new TimeSpan(0, 0, 0, 0, 0)), "Admin", "$2a$11$CWS64rpb86JyrBIj6T/Ok./nWtY.3LwP70jxY5Cr6KJ4WqNmtJadi", null, 1 });
        }
    }
}

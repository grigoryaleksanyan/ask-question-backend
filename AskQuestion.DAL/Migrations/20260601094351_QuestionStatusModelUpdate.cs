using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class QuestionStatusModelUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Questions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuestionStatusTransitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: false),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionStatusTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionStatusTransitions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionStatusTransitions_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionStatusTransitions_ChangedByUserId",
                table: "QuestionStatusTransitions",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionStatusTransitions_QuestionId",
                table: "QuestionStatusTransitions",
                column: "QuestionId");

            migrationBuilder.Sql(
                "UPDATE \"Questions\" SET \"Status\" = 1 WHERE \"Status\" = 2");

            migrationBuilder.Sql(
                "UPDATE \"Questions\" SET \"Status\" = 2 WHERE \"Status\" = 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionStatusTransitions");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Questions");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AskQuestion.DAL.Migrations
{
    /// <inheritdoc />
    public partial class QuestionAreaToFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AreaId",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE ""Questions"" SET ""AreaId"" = a.""Id""
                  FROM ""Areas"" a
                  WHERE ""Questions"".""Area"" = a.""Title""
                    AND ""Questions"".""Area"" IS NOT NULL
                    AND ""Questions"".""Area"" != ''");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "Questions");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_AreaId",
                table: "Questions",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Areas_AreaId",
                table: "Questions",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Areas_AreaId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_AreaId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Questions");

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "Questions",
                type: "text",
                nullable: true);
        }
    }
}

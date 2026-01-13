using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.API.Migrations
{
    /// <inheritdoc />
    public partial class isExtraQuizMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExtraQuiz",
                table: "Scores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Scores",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExtraQuiz",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Scores");
        }
    }
}

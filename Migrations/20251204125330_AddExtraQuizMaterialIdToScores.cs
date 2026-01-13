using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.API.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraQuizMaterialIdToScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExtraQuizMaterialId",
                table: "Scores",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraQuizMaterialId",
                table: "Scores");
        }
    }
}

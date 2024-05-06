using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnEnglish.Migrations.EnglishDb
{
    /// <inheritdoc />
    public partial class removefkfromtestresult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_TestResults_TestResultId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_TestResultId",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "TestResultId",
                table: "UserAnswers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestResultId",
                table: "UserAnswers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_TestResultId",
                table: "UserAnswers",
                column: "TestResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_TestResults_TestResultId",
                table: "UserAnswers",
                column: "TestResultId",
                principalTable: "TestResults",
                principalColumn: "Id");
        }
    }
}

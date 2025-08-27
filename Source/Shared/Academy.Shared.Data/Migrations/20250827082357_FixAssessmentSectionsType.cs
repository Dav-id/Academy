using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Academy.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAssessmentSectionsType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSection_Assessments_AssessmentId",
                table: "AssessmentSection");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSection_Tenants_TenantId",
                table: "AssessmentSection");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSectionQuestion_AssessmentSection_SectionId",
                table: "AssessmentSectionQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSectionQuestion_Assessments_AssessmentId",
                table: "AssessmentSectionQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSectionQuestion_Tenants_TenantId",
                table: "AssessmentSectionQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSectionQuestionAnswers_AssessmentSectionQuestion_~",
                table: "AssessmentSectionQuestionAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSectionQuestionOptions_AssessmentSectionQuestion_~",
                table: "AssessmentSectionQuestionOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssessmentSectionQuestion",
                table: "AssessmentSectionQuestion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssessmentSection",
                table: "AssessmentSection");

            migrationBuilder.RenameTable(
                name: "AssessmentSectionQuestion",
                newName: "AssessmentSectionQuestions");

            migrationBuilder.RenameTable(
                name: "AssessmentSection",
                newName: "AssessmentSections");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentSectionQuestion_TenantId",
                table: "AssessmentSectionQuestions",
                newName: "IX_AssessmentSectionQuestions_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentSectionQuestion_SectionId",
                table: "AssessmentSectionQuestions",
                newName: "IX_AssessmentSectionQuestions_SectionId");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentSectionQuestion_AssessmentId",
                table: "AssessmentSectionQuestions",
                newName: "IX_AssessmentSectionQuestions_AssessmentId");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentSection_TenantId",
                table: "AssessmentSections",
                newName: "IX_AssessmentSections_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentSection_AssessmentId",
                table: "AssessmentSections",
                newName: "IX_AssessmentSections_AssessmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssessmentSectionQuestions",
                table: "AssessmentSectionQuestions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssessmentSections",
                table: "AssessmentSections",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSectionQuestionAnswers_AssessmentSectionQuestions~",
                table: "AssessmentSectionQuestionAnswers",
                column: "QuestionId",
                principalTable: "AssessmentSectionQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSectionQuestionOptions_AssessmentSectionQuestions~",
                table: "AssessmentSectionQuestionOptions",
                column: "QuestionId",
                principalTable: "AssessmentSectionQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSectionQuestions_AssessmentSections_SectionId",
                table: "AssessmentSectionQuestions",
                column: "SectionId",
                principalTable: "AssessmentSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSectionQuestions_Assessments_AssessmentId",
                table: "AssessmentSectionQuestions",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSectionQuestions_Tenants_TenantId",
                table: "AssessmentSectionQuestions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSections_Assessments_AssessmentId",
                table: "AssessmentSections",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSections_Tenants_TenantId",
                table: "AssessmentSections",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSectionQuestionAnswers_AssessmentSectionQuestions~",
                table: "AssessmentSectionQuestionAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSectionQuestionOptions_AssessmentSectionQuestions~",
                table: "AssessmentSectionQuestionOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSectionQuestions_AssessmentSections_SectionId",
                table: "AssessmentSectionQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSectionQuestions_Assessments_AssessmentId",
                table: "AssessmentSectionQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSectionQuestions_Tenants_TenantId",
                table: "AssessmentSectionQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSections_Assessments_AssessmentId",
                table: "AssessmentSections");

            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentSections_Tenants_TenantId",
                table: "AssessmentSections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssessmentSections",
                table: "AssessmentSections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssessmentSectionQuestions",
                table: "AssessmentSectionQuestions");

            migrationBuilder.RenameTable(
                name: "AssessmentSections",
                newName: "AssessmentSection");

            migrationBuilder.RenameTable(
                name: "AssessmentSectionQuestions",
                newName: "AssessmentSectionQuestion");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentSections_TenantId",
                table: "AssessmentSection",
                newName: "IX_AssessmentSection_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentSections_AssessmentId",
                table: "AssessmentSection",
                newName: "IX_AssessmentSection_AssessmentId");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentSectionQuestions_TenantId",
                table: "AssessmentSectionQuestion",
                newName: "IX_AssessmentSectionQuestion_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentSectionQuestions_SectionId",
                table: "AssessmentSectionQuestion",
                newName: "IX_AssessmentSectionQuestion_SectionId");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentSectionQuestions_AssessmentId",
                table: "AssessmentSectionQuestion",
                newName: "IX_AssessmentSectionQuestion_AssessmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssessmentSection",
                table: "AssessmentSection",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssessmentSectionQuestion",
                table: "AssessmentSectionQuestion",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSection_Assessments_AssessmentId",
                table: "AssessmentSection",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSection_Tenants_TenantId",
                table: "AssessmentSection",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSectionQuestion_AssessmentSection_SectionId",
                table: "AssessmentSectionQuestion",
                column: "SectionId",
                principalTable: "AssessmentSection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSectionQuestion_Assessments_AssessmentId",
                table: "AssessmentSectionQuestion",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSectionQuestion_Tenants_TenantId",
                table: "AssessmentSectionQuestion",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSectionQuestionAnswers_AssessmentSectionQuestion_~",
                table: "AssessmentSectionQuestionAnswers",
                column: "QuestionId",
                principalTable: "AssessmentSectionQuestion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentSectionQuestionOptions_AssessmentSectionQuestion_~",
                table: "AssessmentSectionQuestionOptions",
                column: "QuestionId",
                principalTable: "AssessmentSectionQuestion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

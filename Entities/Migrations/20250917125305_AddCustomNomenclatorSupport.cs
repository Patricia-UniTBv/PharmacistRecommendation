using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomNomenclatorSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PharmacistRecommendation",
                table: "Prescription",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotesToDoctor",
                table: "Prescription",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiagnosisMentionedByPatient",
                table: "Prescription",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicamentsMentionedByPacient",
                table: "Prescription",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomNomenclatorName",
                table: "Medication",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LinkedOfficialMedicationId",
                table: "Medication",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medication_LinkedOfficialMedicationId",
                table: "Medication",
                column: "LinkedOfficialMedicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medication_Medication_LinkedOfficialMedicationId",
                table: "Medication",
                column: "LinkedOfficialMedicationId",
                principalTable: "Medication",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medication_Medication_LinkedOfficialMedicationId",
                table: "Medication");

            migrationBuilder.DropIndex(
                name: "IX_Medication_LinkedOfficialMedicationId",
                table: "Medication");

            migrationBuilder.DropColumn(
                name: "MedicamentsMentionedByPacient",
                table: "Prescription");

            migrationBuilder.DropColumn(
                name: "CustomNomenclatorName",
                table: "Medication");

            migrationBuilder.DropColumn(
                name: "LinkedOfficialMedicationId",
                table: "Medication");

            migrationBuilder.AlterColumn<string>(
                name: "PharmacistRecommendation",
                table: "Prescription",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotesToDoctor",
                table: "Prescription",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiagnosisMentionedByPatient",
                table: "Prescription",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministrationModeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdministrationMode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrationMode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Document__3214EC0739146449", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodCIM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Denumire = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DCI = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FormaFarmaceutica = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Concentratia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FirmaProducatoare = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FirmaDetinatoare = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CodATC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ActiuneTerapeutica = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Prescriptie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NrData = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ambalaj = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    VolumAmbalaj = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Valabilitate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Bulina = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Diez = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Stea = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Triunghi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Dreptunghi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    PreviousCodCIM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    DataSource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Manual")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Medicati__3214EC07AAA77029", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CNP = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Birthdate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Patient__3214EC079F46E542", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pharmacy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CUI = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Logo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ConsentTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Pharmacy__3214EC075262534F", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SmtpServer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SmtpPort = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EnableSsl = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EmailCon__3214EC078A630780", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailConfiguration_Pharmacy",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacy",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImportConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    PrescriptionPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReceiptPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ImportCo__3214EC0767366104", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportConfiguration_Pharmacy",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacy",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PharmacyCard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Pharmacy__3214EC078F3535BC", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyCard_Patient",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PharmacyCard_Pharmacy",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacy",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PharmacyId = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PersonalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NCM = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__3214EC0734FF2881", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Pharmacy",
                        column: x => x.PharmacyId,
                        principalTable: "Pharmacy",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Monitoring",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MonitoringDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    Height = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ParametersJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Monitori__3214EC07DFBF32F4", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Monitoring_Patient",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Monitoring_PharmacyCard",
                        column: x => x.CardId,
                        principalTable: "PharmacyCard",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Document__3214EC07A42E023C", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Document_DocumentType",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Document_Patient",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Document_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pharmacist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: true),
                    ActivationCheck = table.Column<bool>(type: "bit", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ncm = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Pharmacist__3214EC07", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pharmacist_User",
                        column: x => x.Id,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MedicationDocument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicationId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Medicati__3214EC077DB018F6", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationDocument_Document",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MedicationDocument_Medication",
                        column: x => x.MedicationId,
                        principalTable: "Medication",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Prescription",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    DocumentId = table.Column<int>(type: "int", nullable: true),
                    Series = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Diagnostic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    FillDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    DiagnosisMentionedByPatient = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Symptoms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Suspicion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PharmacistObservations = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NotesToDoctor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PharmacistRecommendation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PharmaceuticalService = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DoctorStamp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CaregiverName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CaregiverCnp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PatientName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PatientCnp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Prescrip__3214EC07FC595671", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescription_Document",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prescription_Patient",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Assistant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    SupervisorPharmacistId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Assistant__3214EC07", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assistant_Pharmacist",
                        column: x => x.SupervisorPharmacistId,
                        principalTable: "Pharmacist",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assistant_User",
                        column: x => x.Id,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionMedication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrescriptionId = table.Column<int>(type: "int", nullable: false),
                    MedicationId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Morning = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Noon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Evening = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Night = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AdministrationModeId = table.Column<int>(type: "int", nullable: true),
                    IsWithPrescription = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Prescrip__3214EC07825DDC1B", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionMedication_AdministrationMode",
                        column: x => x.AdministrationModeId,
                        principalTable: "AdministrationMode",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PrescriptionMedication_Medication",
                        column: x => x.MedicationId,
                        principalTable: "Medication",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PrescriptionMedication_Prescription",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescription",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assistant_SupervisorPharmacistId",
                table: "Assistant",
                column: "SupervisorPharmacistId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_DocumentTypeId",
                table: "Document",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_PatientId",
                table: "Document",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_UserId",
                table: "Document",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailConfiguration_PharmacyId",
                table: "EmailConfiguration",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportConfiguration_PharmacyId",
                table: "ImportConfiguration",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_Medication_CodCIM",
                table: "Medication",
                column: "CodCIM");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDocument_DocumentId",
                table: "MedicationDocument",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDocument_MedicationId",
                table: "MedicationDocument",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Monitoring_CardId",
                table: "Monitoring",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Monitoring_PatientId",
                table: "Monitoring",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_PersonalId",
                table: "Patient",
                column: "CNP");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyCard_PatientId",
                table: "PharmacyCard",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyCard_PharmacyId",
                table: "PharmacyCard",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_DocumentId",
                table: "Prescription",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_PatientId",
                table: "Prescription",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_SeriesNumber",
                table: "Prescription",
                columns: new[] { "Series", "Number" });

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionMedication_AdministrationModeId",
                table: "PrescriptionMedication",
                column: "AdministrationModeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionMedication_MedicationId",
                table: "PrescriptionMedication",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionMedication_PrescriptionId",
                table: "PrescriptionMedication",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_User_PharmacyId",
                table: "User",
                column: "PharmacyId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true,
                filter: "([Username] IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assistant");

            migrationBuilder.DropTable(
                name: "EmailConfiguration");

            migrationBuilder.DropTable(
                name: "ImportConfiguration");

            migrationBuilder.DropTable(
                name: "MedicationDocument");

            migrationBuilder.DropTable(
                name: "Monitoring");

            migrationBuilder.DropTable(
                name: "PrescriptionMedication");

            migrationBuilder.DropTable(
                name: "Pharmacist");

            migrationBuilder.DropTable(
                name: "PharmacyCard");

            migrationBuilder.DropTable(
                name: "AdministrationMode");

            migrationBuilder.DropTable(
                name: "Medication");

            migrationBuilder.DropTable(
                name: "Prescription");

            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "DocumentType");

            migrationBuilder.DropTable(
                name: "Patient");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Pharmacy");
        }
    }
}

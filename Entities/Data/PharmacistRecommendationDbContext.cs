using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Entities.Data;

public partial class PharmacistRecommendationDbContext : DbContext
{
    public PharmacistRecommendationDbContext()
    {
    }

    public PharmacistRecommendationDbContext(DbContextOptions<PharmacistRecommendationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Administrator> Administrators { get; set; }
    public virtual DbSet<Assistant> Assistants { get; set; }
    public virtual DbSet<CardioMonitoring> CardioMonitorings { get; set; }
    public virtual DbSet<DiabetesMonitoring> DiabetesMonitorings { get; set; }
    public virtual DbSet<Doctor> Doctors { get; set; }
    public virtual DbSet<DoctorMedication> DoctorMedications { get; set; }
    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<DocumentType> DocumentTypes { get; set; }
    public virtual DbSet<EmailConfiguration> EmailConfigurations { get; set; }
    public virtual DbSet<ImportConfiguration> ImportConfigurations { get; set; }
    public virtual DbSet<Medication> Medications { get; set; }
    public virtual DbSet<MedicationDocument> MedicationDocuments { get; set; }
    public virtual DbSet<Monitoring> Monitorings { get; set; }
    public virtual DbSet<Patient> Patients { get; set; }
    public virtual DbSet<Pharmacist> Pharmacists { get; set; }
    public virtual DbSet<Pharmacy> Pharmacies { get; set; }
    public virtual DbSet<PharmacyCard> PharmacyCards { get; set; }
    public virtual DbSet<Prescription> Prescriptions { get; set; }
    public virtual DbSet<PrescriptionMedication> PrescriptionMedications { get; set; }
    public virtual DbSet<TemperatureMonitoring> TemperatureMonitorings { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Only configure if not already configured (for development scenarios)
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=PharmacistRecommendationDB;Trusted_Connection=true;TrustServerCertificate=true;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Administ__3214EC07312306EA");
            entity.ToTable("Administrator");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Administrator)
                .HasForeignKey<Administrator>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Administrator_User");
        });

        modelBuilder.Entity<Assistant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Assistan__3214EC07261C8567");
            entity.ToTable("Assistant");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Assistant)
                .HasForeignKey<Assistant>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assistant_User");
            entity.HasOne(d => d.SupervisorPharmacist).WithMany(p => p.Assistants)
                .HasForeignKey(d => d.SupervisorPharmacistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assistant_Pharmacist");
        });

        modelBuilder.Entity<CardioMonitoring>(entity =>
        {
            entity.HasKey(e => e.MonitoringId).HasName("PK__CardioMo__CAC3C077D583B478");
            entity.ToTable("CardioMonitoring");
            entity.Property(e => e.MonitoringId).ValueGeneratedNever().HasColumnName("MonitoringID");
            entity.Property(e => e.MaxBloodPressure).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MinBloodPressure).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PulseOximetry).HasColumnType("decimal(5, 2)");
            entity.HasOne(d => d.Monitoring).WithOne(p => p.CardioMonitoring)
                .HasForeignKey<CardioMonitoring>(d => d.MonitoringId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CardioMon__Monit__76969D2E");
        });

        modelBuilder.Entity<DiabetesMonitoring>(entity =>
        {
            entity.HasKey(e => e.MonitoringId).HasName("PK__Diabetes__CAC3C077427749C7");
            entity.ToTable("DiabetesMonitoring");
            entity.Property(e => e.MonitoringId).ValueGeneratedNever().HasColumnName("MonitoringID");
            entity.Property(e => e.Glucose).HasColumnType("decimal(5, 2)");
            entity.HasOne(d => d.Monitoring).WithOne(p => p.DiabetesMonitoring)
                .HasForeignKey<DiabetesMonitoring>(d => d.MonitoringId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DiabetesM__Monit__7D439ABD");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Doctor__3214EC07B25007FB");
            entity.ToTable("Doctor");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Specialization).HasMaxLength(100);
        });

        modelBuilder.Entity<DoctorMedication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DoctorMe__3214EC07CCAC5E47");
            entity.ToTable("DoctorMedication");
            entity.HasOne(d => d.Doctor).WithMany(p => p.DoctorMedications)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DoctorMedication_Doctor");
            entity.HasOne(d => d.Medication).WithMany(p => p.DoctorMedications)
                .HasForeignKey(d => d.MedicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DoctorMedication_Medication");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Document__3214EC07A42E023C");
            entity.ToTable("Document");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.HasOne(d => d.Assistant).WithMany(p => p.Documents)
                .HasForeignKey(d => d.AssistantId)
                .HasConstraintName("FK_Document_Assistant");
            entity.HasOne(d => d.DocumentType).WithMany(p => p.Documents)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Document_DocumentType");
            entity.HasOne(d => d.Patient).WithMany(p => p.Documents)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_Document_Patient");
            entity.HasOne(d => d.Pharmacist).WithMany(p => p.Documents)
                .HasForeignKey(d => d.PharmacistId)
                .HasConstraintName("FK_Document_Pharmacist");
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Document__3214EC0739146449");
            entity.ToTable("DocumentType");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<EmailConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EmailCon__3214EC078A630780");
            entity.ToTable("EmailConfiguration");
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.SmtpPort).HasMaxLength(10);
            entity.Property(e => e.SmtpServer).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.HasOne(d => d.Pharmacy).WithMany(p => p.EmailConfigurations)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmailConfiguration_Pharmacy");
        });

        modelBuilder.Entity<ImportConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ImportCo__3214EC0767366104");
            entity.ToTable("ImportConfiguration");
            entity.Property(e => e.ImportPath).HasMaxLength(255);
            entity.Property(e => e.PrescriptionPath).HasMaxLength(255);
            entity.HasOne(d => d.Pharmacy).WithMany(p => p.ImportConfigurations)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImportConfiguration_Pharmacy");
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medicati__3214EC07AAA77029");
            entity.ToTable("Medication");
            entity.HasIndex(e => e.CodCIM, "IX_Medication_CodCIM");
                
           
            entity.Property(e => e.CodCIM).HasMaxLength(50);
            entity.Property(e => e.Denumire).HasMaxLength(500);
            entity.Property(e => e.DCI).HasMaxLength(255);
            entity.Property(e => e.FormaFarmaceutica).HasMaxLength(255);
            entity.Property(e => e.Concentratia).HasMaxLength(100);
            entity.Property(e => e.FirmaProducatoare).HasMaxLength(255); 
            entity.Property(e => e.FirmaDetinatoare).HasMaxLength(255); 
            entity.Property(e => e.CodATC).HasMaxLength(50);
            entity.Property(e => e.ActiuneTerapeutica).HasMaxLength(255);
            entity.Property(e => e.Prescriptie).HasMaxLength(100);
            entity.Property(e => e.NrData).HasMaxLength(100);
            entity.Property(e => e.Ambalaj).HasMaxLength(255);
            entity.Property(e => e.VolumAmbalaj).HasMaxLength(100);
            entity.Property(e => e.Valabilitate).HasMaxLength(100);
            entity.Property(e => e.Bulina).HasMaxLength(50);
            entity.Property(e => e.Diez).HasMaxLength(50);
            entity.Property(e => e.Stea).HasMaxLength(50);
            entity.Property(e => e.Triunghi).HasMaxLength(50);
            entity.Property(e => e.Dreptunghi).HasMaxLength(50);         
            entity.Property(e => e.DataSource).HasMaxLength(50).HasDefaultValue("Manual");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PreviousCodCIM).HasMaxLength(50); 
        });

        modelBuilder.Entity<MedicationDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medicati__3214EC077DB018F6");
            entity.ToTable("MedicationDocument");
            entity.HasOne(d => d.Document).WithMany(p => p.MedicationDocuments)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicationDocument_Document");
            entity.HasOne(d => d.Medication).WithMany(p => p.MedicationDocuments)
                .HasForeignKey(d => d.MedicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicationDocument_Medication");
        });

        modelBuilder.Entity<Monitoring>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Monitori__3214EC07DFBF32F4");
            entity.ToTable("Monitoring");
            entity.Property(e => e.Height).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MonitoringDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(200);
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");
            entity.HasOne(d => d.Card).WithMany(p => p.Monitorings)
                .HasForeignKey(d => d.CardId)
                .HasConstraintName("FK_Monitoring_PharmacyCard");
            entity.HasOne(d => d.Patient).WithMany(p => p.Monitorings)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Monitoring_Patient");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Patient__3214EC079F46E542");
            entity.ToTable("Patient");
            entity.HasIndex(e => e.Cnp, "IX_Patient_PersonalId");
            entity.Property(e => e.Cid).HasMaxLength(10).HasColumnName("CID");
            entity.Property(e => e.Cnp).HasMaxLength(20).HasColumnName("CNP");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Pharmacist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pharmaci__3214EC07D59DC098");
            entity.ToTable("Pharmacist");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ActivationCheck).HasDefaultValue(false);
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Ncm).HasMaxLength(50).HasColumnName("NCM");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Pharmacist)
                .HasForeignKey<Pharmacist>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pharmacist_User");
        });

        modelBuilder.Entity<Pharmacy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pharmacy__3214EC075262534F");
            entity.ToTable("Pharmacy");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Logo).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.TaxId).HasMaxLength(20);
        });

        modelBuilder.Entity<PharmacyCard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pharmacy__3214EC078F3535BC");
            entity.ToTable("PharmacyCard");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.IssueDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.HasOne(d => d.Patient).WithMany(p => p.PharmacyCards)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PharmacyCard_Patient");
            entity.HasOne(d => d.Pharmacy).WithMany(p => p.PharmacyCards)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PharmacyCard_Pharmacy");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Prescrip__3214EC07FC595671");
            entity.ToTable("Prescription");
            entity.HasIndex(e => new { e.Series, e.Number }, "IX_Prescription_SeriesNumber");
            entity.Property(e => e.Diagnostic).HasMaxLength(200);
            entity.Property(e => e.FillDate).HasColumnType("datetime");
            entity.Property(e => e.IssueDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.Number).HasMaxLength(20);
            entity.Property(e => e.Series).HasMaxLength(20);
            entity.HasOne(d => d.Doctor).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prescription_Doctor");
            entity.HasOne(d => d.Document).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.DocumentId)
                .HasConstraintName("FK_Prescription_Document");
            entity.HasOne(d => d.Patient).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prescription_Patient");
        });

        modelBuilder.Entity<PrescriptionMedication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Prescrip__3214EC07825DDC1B");
            entity.ToTable("PrescriptionMedication");
            entity.Property(e => e.Dosage).HasMaxLength(100);
            entity.Property(e => e.Instructions).HasMaxLength(200);
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.HasOne(d => d.Medication).WithMany(p => p.PrescriptionMedications)
                .HasForeignKey(d => d.MedicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrescriptionMedication_Medication");
            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionMedications)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrescriptionMedication_Prescription");
        });

        modelBuilder.Entity<TemperatureMonitoring>(entity =>
        {
            entity.HasKey(e => e.MonitoringId).HasName("PK__Temperat__CAC3C0779B3B23A7");
            entity.ToTable("TemperatureMonitoring");
            entity.Property(e => e.MonitoringId).ValueGeneratedNever().HasColumnName("MonitoringID");
            entity.Property(e => e.Temperature).HasColumnType("decimal(5, 2)");
            entity.HasOne(d => d.Monitoring).WithOne(p => p.TemperatureMonitoring)
                .HasForeignKey<TemperatureMonitoring>(d => d.MonitoringId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Temperatu__Monit__00200768");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC0734FF2881");
            entity.ToTable("User");
            entity.HasIndex(e => e.Username, "IX_User_Username")
                .IsUnique()
                .HasFilter("([Username] IS NOT NULL)");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.PersonalId).HasMaxLength(20);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.UserType).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.HasOne(d => d.Pharmacy).WithMany(p => p.Users)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Pharmacy");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
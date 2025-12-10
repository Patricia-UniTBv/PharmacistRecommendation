using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Data.Models;

namespace Pharmacy.Data.Data;

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

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=Patricia-Anelis\\PHARMACYREC;Database=PharmacistRecommendationDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Administ__3214EC07312306EA");

            entity.ToTable("Administrator");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Administrator)
                .HasForeignKey<Administrator>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Administrator_User");
        });

        modelBuilder.Entity<Assistant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Assistan__3214EC07261C8567");

            entity.ToTable("Assistant");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Assistant)
                .HasForeignKey<Assistant>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assistant_User");

            entity.HasOne(d => d.SupervisorPharmacist).WithMany(p => p.Assistants)
                .HasForeignKey(d => d.SupervisorPharmacistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Assistant_Pharmacist");
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

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
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

            entity.HasOne(d => d.Patient).WithMany(p => p.Document

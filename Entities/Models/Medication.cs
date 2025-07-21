using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("Medication")]
    public class Medication
    {
        [Key]
        public int Id { get; set; }

        [Column("CodCIM")]
        [StringLength(50)]
        public string? CodCIM { get; set; }

        [Column("Denumire")]
        [StringLength(500)]
        public string? Denumire { get; set; }

        [Column("DCI")]
        [StringLength(255)]
        public string? DCI { get; set; }

        [Column("FormaFarmaceutica")]
        [StringLength(255)]
        public string? FormaFarmaceutica { get; set; }

        [Column("Concentratia")]
        [StringLength(100)]
        public string? Concentratia { get; set; }

        [Column("FirmaProducatoare")]  
        [StringLength(255)]
        public string? FirmaProducatoare { get; set; }

        [Column("FirmaDetinatoare")] 
        [StringLength(255)]
        public string? FirmaDetinatoare { get; set; }

        [Column("CodATC")]
        [StringLength(50)]
        public string? CodATC { get; set; }

        [Column("ActiuneTerapeutica")]
        [StringLength(255)]
        public string? ActiuneTerapeutica { get; set; }

        [Column("Prescriptie")]
        [StringLength(100)]
        public string? Prescriptie { get; set; }

        [Column("NrData")]
        [StringLength(100)]
        public string? NrData { get; set; }

        [Column("Ambalaj")]
        [StringLength(255)]
        public string? Ambalaj { get; set; }

        [Column("VolumAmbalaj")]
        [StringLength(100)]
        public string? VolumAmbalaj { get; set; }

        [Column("Valabilitate")]
        [StringLength(100)]
        public string? Valabilitate { get; set; }

        [Column("Bulina")]
        [StringLength(50)]
        public string? Bulina { get; set; }

        [Column("Diez")]
        [StringLength(50)]
        public string? Diez { get; set; }

        [Column("Stea")]
        [StringLength(50)]
        public string? Stea { get; set; }

        [Column("Triunghi")]
        [StringLength(50)]
        public string? Triunghi { get; set; }

        [Column("Dreptunghi")]
        [StringLength(50)]
        public string? Dreptunghi { get; set; }

        //System columns
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("PreviousCodCIM")]
        [StringLength(50)]
        public string? PreviousCodCIM { get; set; }

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column("DataSource")]
        [StringLength(50)]
        public string DataSource { get; set; } = "Manual";

        // Navigation properties
        public virtual ICollection<DoctorMedication> DoctorMedications { get; set; } = new List<DoctorMedication>();
        public virtual ICollection<MedicationDocument> MedicationDocuments { get; set; } = new List<MedicationDocument>();
        public virtual ICollection<PrescriptionMedication> PrescriptionMedications { get; set; } = new List<PrescriptionMedication>();
    }
}
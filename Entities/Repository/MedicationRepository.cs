using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Repository
{
    public class MedicationRepository : IMedicationRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public MedicationRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Medication>> GetAllAsync()
        {
            return await _context.Medications
                .OrderBy(m => m.Denumire)
                .ToListAsync();
        }

        public async Task<List<Medication>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync();
            }

            var term = searchTerm.ToLower().Trim();

            return await _context.Medications
                .Where(m =>
                    (m.Denumire != null && m.Denumire.ToLower().Contains(term)) ||
                    (m.CodCIM != null && m.CodCIM.ToLower().Contains(term)) ||
                    (m.CodATC != null && m.CodATC.ToLower().Contains(term)) ||
                    (m.DCI != null && m.DCI.ToLower().Contains(term)) ||
                    (m.FirmaProducatoare != null && m.FirmaProducatoare.ToLower().Contains(term)) ||
                    (m.FirmaDetinatoare != null && m.FirmaDetinatoare.ToLower().Contains(term)) ||
                    (m.FormaFarmaceutica != null && m.FormaFarmaceutica.ToLower().Contains(term)))
                .OrderBy(m => m.Denumire)
                .Take(100) 
                .ToListAsync();
        }

        public async Task<Medication> GetByIdAsync(int id)
        {
            return await _context.Medications
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Medication> AddAsync(Medication medication)
        {
            if (medication == null)
                throw new ArgumentNullException(nameof(medication));

            medication.CreatedAt = DateTime.Now;
            medication.UpdatedAt = DateTime.Now;

            _context.Medications.Add(medication);
            await _context.SaveChangesAsync();

            return medication;
        }

        public async Task<Medication> UpdateAsync(Medication medication)
        {
            if (medication == null)
                throw new ArgumentNullException(nameof(medication));

            var existingMedication = await _context.Medications
                .FirstOrDefaultAsync(m => m.Id == medication.Id);

            if (existingMedication == null)
                throw new Exception("Medication not found");

            existingMedication.Denumire = medication.Denumire;
            existingMedication.CodCIM = medication.CodCIM;
            existingMedication.CodATC = medication.CodATC;
            existingMedication.DCI = medication.DCI;
            existingMedication.FormaFarmaceutica = medication.FormaFarmaceutica;
            existingMedication.Concentratia = medication.Concentratia;
            existingMedication.FirmaProducatoare = medication.FirmaProducatoare;
            existingMedication.FirmaDetinatoare = medication.FirmaDetinatoare;
            existingMedication.ActiuneTerapeutica = medication.ActiuneTerapeutica;
            existingMedication.Prescriptie = medication.Prescriptie;
            existingMedication.NrData = medication.NrData;
            existingMedication.Ambalaj = medication.Ambalaj;
            existingMedication.VolumAmbalaj = medication.VolumAmbalaj;
            existingMedication.Valabilitate = medication.Valabilitate;
            existingMedication.Bulina = medication.Bulina;
            existingMedication.Diez = medication.Diez;
            existingMedication.Stea = medication.Stea;
            existingMedication.Triunghi = medication.Triunghi;
            existingMedication.Dreptunghi = medication.Dreptunghi;
            existingMedication.PreviousCodCIM = medication.PreviousCodCIM;
            existingMedication.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return existingMedication;
        }

        public async Task DeleteAsync(int id)
        {
            var medication = await _context.Medications
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medication == null)
                throw new Exception("Medication not found");

            _context.Medications.Remove(medication);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Medications
                .AnyAsync(m => m.Id == id);
        }

        public async Task<List<Medication>> GetByCodCIMAsync(string codCIM)
        {
            if (string.IsNullOrWhiteSpace(codCIM))
                return new List<Medication>();

            return await _context.Medications
                .Where(m => m.CodCIM == codCIM)
                .OrderBy(m => m.Denumire)
                .ToListAsync();
        }

        public async Task<List<Medication>> GetByAtcCodeAsync(string atcCode)
        {
            if (string.IsNullOrWhiteSpace(atcCode))
                return new List<Medication>();

            return await _context.Medications
                .Where(m => m.CodATC == atcCode)
                .OrderBy(m => m.Denumire)
                .ToListAsync();
        }
        public async Task<List<Medication>> GetByDataSourceAsync(string dataSource)
        {
            if (string.IsNullOrWhiteSpace(dataSource))
                return new List<Medication>();

            return await _context.Medications
                .Where(m => m.DataSource == dataSource)
                .OrderBy(m => m.Denumire)
                .ToListAsync();
        }

        public async Task<List<Medication>> BatchAddAsync(List<Medication> medications)
        {
            if (medications == null || !medications.Any())
                return new List<Medication>();

            foreach (var medication in medications)
            {
                medication.CreatedAt = DateTime.Now;
                medication.UpdatedAt = DateTime.Now;
            }

            _context.Medications.AddRange(medications);
            await _context.SaveChangesAsync();

            return medications;
        }

        public async Task<List<Medication>> BatchUpdateAsync(List<Medication> medications)
        {
            if (medications == null || !medications.Any())
                return new List<Medication>();

            foreach (var medication in medications)
            {
                medication.UpdatedAt = DateTime.Now;
                _context.Medications.Update(medication);
            }

            await _context.SaveChangesAsync();
            return medications;
        }

        public async Task<Medication> UpdateCodCIMAsync(int medicationId, string newCodCIM, string oldCodCIM)
        {
            var medication = await _context.Medications.FindAsync(medicationId);
            if (medication == null)
                throw new Exception("Medication not found");

            medication.PreviousCodCIM = oldCodCIM;
            medication.CodCIM = newCodCIM;
            medication.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return medication;
        }
    }
}
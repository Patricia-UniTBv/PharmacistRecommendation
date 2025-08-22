using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Repository
{
    public class PrescriptionMedicationRepository: IPrescriptionMedicationRepository
    {
        private readonly PharmacistRecommendationDbContext _context;

        public PrescriptionMedicationRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PrescriptionMedication>> GetAllByPrescriptionIdAsync(int prescriptionId)
            => await _context.PrescriptionMedications
                .Where(m => m.PrescriptionId == prescriptionId)
                .ToListAsync();

        public async Task AddAsync(PrescriptionMedication medication)
        {
            _context.PrescriptionMedications.Add(medication);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PrescriptionMedication medication)
        {
            _context.PrescriptionMedications.Update(medication);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var medication = await _context.PrescriptionMedications.FindAsync(id);
            if (medication != null)
            {
                _context.PrescriptionMedications.Remove(medication);
                await _context.SaveChangesAsync();
            }
        }
    }
}

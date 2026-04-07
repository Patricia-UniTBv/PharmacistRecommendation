using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Entities.Repository
{
    public class PrescriptionRepository: IPrescriptionRepository
    {
        private readonly PharmacistRecommendationDbContext _context;
        public PrescriptionRepository(PharmacistRecommendationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Prescription>> GetAllAsync()
            => await _context.Prescriptions
                .Include(p => p.PrescriptionMedications)
                .Include(p => p.Patient)
                    .ThenInclude(patient => patient.PharmacyCards)
                .ToListAsync();

        public async Task<Prescription?> GetByIdAsync(int id)
            => await _context.Prescriptions
                .Include(p => p.PrescriptionMedications)
                .Include(p => p.Patient)
                    .ThenInclude(patient => patient.PharmacyCards)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<List<Prescription>> GetByPatientCnpOrCidAsync(string cnpOrCid)
            => await _context.Prescriptions
                .Include(p => p.Patient)
                .Where(p => p.Patient.Cnp == cnpOrCid || p.Patient.Cid == cnpOrCid)
                .ToListAsync();

        public async Task AddAsync(Prescription prescription)
        {
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Prescription prescription)
        {
            var existing = await _context.Prescriptions.FindAsync(prescription.Id);
            if (existing == null) return;
            _context.Entry(existing).CurrentValues.SetValues(prescription);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Prescription>> GetByPatientIdAsync(int patientId)
            => await _context.Prescriptions
                .Where(p => p.PatientId == patientId)
                .ToListAsync();

        public async Task UpdatePatientNameAsync(int patientId, string newPatientName)
        {
            var prescriptions = await _context.Prescriptions
                .Where(p => p.PatientId == patientId)
                .ToListAsync();
            foreach (var p in prescriptions)
                p.PatientName = newPatientName;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.PrescriptionMedications)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription != null)
            {
                _context.PrescriptionMedications.RemoveRange(prescription.PrescriptionMedications);
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
            }
        }
    }
}

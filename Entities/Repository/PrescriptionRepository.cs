using Entities.Data;
using Entities.Models;
using Entities.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .ToListAsync();

        public async Task<Prescription?> GetByIdAsync(int id)
            => await _context.Prescriptions
                .Include(p => p.PrescriptionMedications)
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
            _context.Prescriptions.Update(prescription);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
            }
        }
    }
}

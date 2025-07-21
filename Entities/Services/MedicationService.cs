using Entities.Models;
using Entities.Services.Interfaces;
using Entities.Repository.Interfaces;

namespace Entities.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly IMedicationRepository _medicationRepository;

        public MedicationService(IMedicationRepository medicationRepository)
        {
            _medicationRepository = medicationRepository;
        }

        public async Task<List<Medication>> GetAllMedicationsAsync()
        {
            try
            {
                return await _medicationRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving medications: {ex.Message}", ex);
            }
        }

        public async Task<List<Medication>> SearchMedicationsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllMedicationsAsync();
                }

                return await _medicationRepository.SearchAsync(searchTerm);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching medications: {ex.Message}", ex);
            }
        }

        public async Task<Medication> AddMedicationAsync(Medication medication)
        {
            try
            {
                if (medication == null)
                    throw new ArgumentNullException(nameof(medication));

                medication.CreatedAt = DateTime.Now;
                medication.UpdatedAt = DateTime.Now;
                medication.DataSource = "Manual";

                return await _medicationRepository.AddAsync(medication);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding medication: {ex.Message}", ex);
            }
        }

        public async Task<Medication> UpdateMedicationAsync(Medication medication)
        {
            try
            {
                if (medication == null)
                    throw new ArgumentNullException(nameof(medication));

                medication.UpdatedAt = DateTime.Now;
                return await _medicationRepository.UpdateAsync(medication);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating medication: {ex.Message}", ex);
            }
        }

        public async Task DeleteMedicationAsync(int id)
        {
            try
            {
                await _medicationRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting medication: {ex.Message}", ex);
            }
        }
    }
}
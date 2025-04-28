using Entities.Models;
using Entities.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Services
{
    public class PatientService: IPatientService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PatientService(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<IEnumerable<Patient>> GetPatientsAsync()
        {
            var response = await _httpClient.GetAsync("/api/patients");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<Patient>>();
            }

            return new List<Patient>();
        }

        public async Task<Patient> GetPatientAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/patients/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Patient>();
            }

            return null;
        }

        public async Task<bool> AddPatientAsync(Patient patient)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/patients", patient);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePatientAsync(Patient patient)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/patients/{patient.Id}", patient);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/patients/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

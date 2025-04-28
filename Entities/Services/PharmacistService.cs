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
    public class PharmacistService: IPharmacistService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PharmacistService(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<IEnumerable<Pharmacist>> GetPharmacistsAsync()
        {
            var response = await _httpClient.GetAsync("/api/pharmacists");

            if (response.IsSuccessStatusCode)
            {
                return await response!.Content.ReadFromJsonAsync<IEnumerable<Pharmacist>>();
            }

            return new List<Pharmacist>();
        }

        public async Task<Pharmacist> GetPharmacistAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/pharmacists/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Pharmacist>();
            }

            return null;
        }

        public async Task<bool> AddPharmacistAsync(Pharmacist pharmacist)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/pharmacists", pharmacist);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePharmacistAsync(Pharmacist pharmacist)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/pharmacists/{pharmacist.Id}", pharmacist);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePharmacistAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/pharmacists/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

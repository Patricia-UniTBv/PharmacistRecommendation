using Entities.Services.Interfaces;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Services
{
    public class MauiSecureStorageService : ISecureStorageService
    {
        public async Task SetAsync(string key, string value)
        {
            await Microsoft.Maui.Storage.SecureStorage.SetAsync(key, value);
        }

        public async Task<string?> GetAsync(string key)
        {
            return await Microsoft.Maui.Storage.SecureStorage.GetAsync(key);
        }

        public void Remove(string key)
        {
            Microsoft.Maui.Storage.SecureStorage.Remove(key);
        }
    }
}
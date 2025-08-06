using System.Threading.Tasks;

namespace Entities.Services.Interfaces
{
    public interface ISecureStorageService
    {
        Task SetAsync(string key, string value);
        Task<string?> GetAsync(string key);
        void Remove(string key);
    }
}
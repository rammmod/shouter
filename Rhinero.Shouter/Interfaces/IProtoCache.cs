using System.Reflection;

namespace Rhinero.Shouter.Interfaces
{
    public interface IProtoCache
    {
        Task<KeyValuePair<string, string>> GetAsync(string key, CancellationToken cancellationToken);
        ICollection<string> GetAllKeys();
        Task AddAsync(string fileName, string content, CancellationToken cancellationToken);
        Task UpdateAsync(string fileName, string content, CancellationToken cancellationToken);
        void DeleteAsync(string fileName);
    }
}

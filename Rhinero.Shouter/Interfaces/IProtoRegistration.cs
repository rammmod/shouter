namespace Rhinero.Shouter.Interfaces
{
    public interface IProtoRegistration
    {
        Task<KeyValuePair<string, string>> GetAsync(string fileName, CancellationToken cancellationToken);
        Task<ICollection<string>> GetAllFileNamesAsync(CancellationToken cancellationToken);
        Task AddAsync(IFormFile message, CancellationToken cancellationToken);
        Task PutAsync(IFormFile message, CancellationToken cancellationToken);
        Task DeleteAsync(string fileName, CancellationToken cancellationToken);
    }
}

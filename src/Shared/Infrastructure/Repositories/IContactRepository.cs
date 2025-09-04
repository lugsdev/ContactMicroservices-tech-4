using ContactModels;

namespace Infrastructure.Repositories
{
    public interface IContactRepository
    {
        Task<Contact?> GetByIdAsync(int id);
        Task<IEnumerable<Contact>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);
        Task<int> GetTotalCountAsync(string? search = null);
        Task<Contact> CreateAsync(Contact contact);
        Task<Contact> UpdateAsync(Contact contact);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<Contact?> GetByEmailAsync(string email);
    }
}


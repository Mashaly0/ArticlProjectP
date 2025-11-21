// Interfaces/IAuthorRepository.cs - يبقى كما هو
using ArticlProject.Models;

namespace ArticlProject.Interfaces
{
    public interface IAuthorRepository
    {
        Task<IEnumerable<Author>> GetAllAsync();
        Task<Author?> GetByIdAsync(int id);
        Task<Author> CreateAsync(Author author);
        Task<Author> UpdateAsync(Author author);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
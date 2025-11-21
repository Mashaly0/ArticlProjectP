// Interfaces/IArticleRepository.cs - إضافة دالة جديدة
using ArticlProject.Models;

namespace ArticlProject.Interfaces
{
    public interface IArticleRepository
    {
        Task<IEnumerable<Article>> GetAllAsync();
        Task<Article?> GetByIdAsync(int id);
        Task<Article> CreateAsync(Article article);
        Task<Article> UpdateAsync(Article article);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Article>> GetArticlesByUserIdAsync(string userId); // إضافة جديدة
    }
}
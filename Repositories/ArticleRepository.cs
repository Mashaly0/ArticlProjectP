// Repositories/ArticleRepository.cs - تحديث
using ArticlProject.Data;
using ArticlProject.Interfaces;
using ArticlProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ArticlProject.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly ApplicationDbContext _context;

        public ArticleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Article>> GetAllAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<Article?> GetByIdAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.ArticleId == id);
        }

        public async Task<IEnumerable<Article>> GetArticlesByUserIdAsync(string userId)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<Article> CreateAsync(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Article> UpdateAsync(Article article)
        {
            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task DeleteAsync(int id)
        {
            var article = await GetByIdAsync(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Articles.AnyAsync(a => a.ArticleId == id);
        }
    }
}

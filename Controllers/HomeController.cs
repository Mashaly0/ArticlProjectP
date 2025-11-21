// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArticlProject.Data;
using ArticlProject.Models;

namespace ArticlProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var articles = await _context.Articles
                .Include(a => a.Author)
                .OrderByDescending(a => a.CreatedDate)
                .Take(6)
                .ToListAsync();

            var authorsCount = await _context.Authors.CountAsync();
            ViewBag.AuthorsCount = authorsCount;

            return View(articles);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
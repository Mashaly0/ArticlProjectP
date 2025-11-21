// Controllers/ArticlesController.cs
using Microsoft.AspNetCore.Mvc;
using ArticlProject.Models;
using ArticlProject.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ArticlProject.Controllers
{
    [Authorize] // يتطلب تسجيل الدخول لجميع العمليات
    public class ArticlesController : Controller
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;

        public ArticlesController(IArticleRepository articleRepository,
                                IAuthorRepository authorRepository,
                                IWebHostEnvironment webHostEnvironment,
                                UserManager<IdentityUser> userManager)
        {
            _articleRepository = articleRepository;
            _authorRepository = authorRepository;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // GET: Articles
        [AllowAnonymous] // السماح للجميع بمشاهدة المقالات
        public async Task<IActionResult> Index()
        {
            var articles = await _articleRepository.GetAllAsync();
            return View(articles);
        }

        // GET: Articles/Details/5
        [AllowAnonymous] // السماح للجميع بمشاهدة التفاصيل
        public async Task<IActionResult> Details(int id)
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return View(article);
        }

        // GET: Articles/Create
        public async Task<IActionResult> Create()
        {
            await LoadAuthors();
            return View();
        }

        // POST: Articles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Article article)
        {
            if (ModelState.IsValid)
            {
                // الحصول على المستخدم الحالي
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge(); // إعادة توجيه لصفحة تسجيل الدخول
                }

                // تعيين UserId للمقال
                article.UserId = user.Id;

                // معالجة رفع الصورة
                if (article.ImageFile != null && article.ImageFile.Length > 0)
                {
                    article.Image = await SaveImage(article.ImageFile);
                }
                else
                {
                    article.Image = string.Empty;
                }

                article.CreatedDate = DateTime.Now;
                await _articleRepository.CreateAsync(article);
                return RedirectToAction(nameof(Index));
            }
            await LoadAuthors();
            return View(article);
        }

        // GET: Articles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            // التحقق من أن المستخدم هو صاحب المقال
            var user = await _userManager.GetUserAsync(User);
            if (article.UserId != user?.Id && !User.IsInRole("Admin"))
            {
                return Forbid(); // منع الوصول
            }

            await LoadAuthors();
            return View(article);
        }

        // POST: Articles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Article article)
        {
            if (id != article.ArticleId)
            {
                return NotFound();
            }

            // التحقق من الصلاحية قبل التعديل
            var existingArticle = await _articleRepository.GetByIdAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (existingArticle?.UserId != user?.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (existingArticle == null)
                    {
                        return NotFound();
                    }

                    // الحفاظ على بيانات المستخدم الأصلية
                    article.UserId = existingArticle.UserId;
                    article.CreatedDate = existingArticle.CreatedDate;

                    // معالجة رفع الصورة الجديدة
                    if (article.ImageFile != null && article.ImageFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(existingArticle.Image))
                        {
                            DeleteImage(existingArticle.Image);
                        }
                        article.Image = await SaveImage(article.ImageFile);
                    }
                    else
                    {
                        article.Image = existingArticle.Image;
                    }

                    await _articleRepository.UpdateAsync(article);
                }
                catch (Exception)
                {
                    if (!await _articleRepository.ExistsAsync(article.ArticleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            await LoadAuthors();
            return View(article);
        }

        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            // التحقق من الصلاحية قبل الحذف
            var user = await _userManager.GetUserAsync(User);
            if (article.UserId != user?.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article != null)
            {
                // التحقق من الصلاحية قبل الحذف
                var user = await _userManager.GetUserAsync(User);
                if (article.UserId != user?.Id && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // حذف الصورة المرتبطة
                if (!string.IsNullOrEmpty(article.Image))
                {
                    DeleteImage(article.Image);
                }
                await _articleRepository.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Articles/MyArticles - عرض مقالات المستخدم فقط
        public async Task<IActionResult> MyArticles()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var myArticles = await _articleRepository.GetArticlesByUserIdAsync(user.Id);
            return View(myArticles);
        }

        private async Task LoadAuthors()
        {
            var authors = await _authorRepository.GetAllAsync();
            ViewBag.Authors = new SelectList(authors, "AuthorId", "Name");
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        private void DeleteImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return;

            string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", imageName);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
    }
}
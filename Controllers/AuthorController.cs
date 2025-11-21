// Controllers/AuthorsController.cs
using Microsoft.AspNetCore.Mvc;
using ArticlProject.Models;
using ArticlProject.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace ArticlProject.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AuthorsController(IAuthorRepository authorRepository, IWebHostEnvironment webHostEnvironment)
        {
            _authorRepository = authorRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            var authors = await _authorRepository.GetAllAsync();
            return View(authors);
        }

        // GET: Authors/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Author author)
        {
            if (ModelState.IsValid)
            {
                // معالجة رفع الصورة
                if (author.ImageFile != null)
                {
                    author.Image = await SaveImage(author.ImageFile);
                }

                await _authorRepository.CreateAsync(author);
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        // POST: Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            if (id != author.AuthorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // معالجة رفع الصورة الجديدة
                    if (author.ImageFile != null)
                    {
                        // حذف الصورة القديمة إذا كانت موجودة
                        if (!string.IsNullOrEmpty(author.Image))
                        {
                            DeleteImage(author.Image);
                        }
                        author.Image = await SaveImage(author.ImageFile);
                    }

                    await _authorRepository.UpdateAsync(author);
                }
                catch (Exception)
                {
                    if (!await _authorRepository.ExistsAsync(author.AuthorId))
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
            return View(author);
        }

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author != null)
            {
                // حذف الصورة المرتبطة
                if (!string.IsNullOrEmpty(author.Image))
                {
                    DeleteImage(author.Image);
                }
                await _authorRepository.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

            // إنشاء المجلد إذا لم يكن موجوداً
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        private void DeleteImage(string imageName)
        {
            string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", imageName);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
    }
}
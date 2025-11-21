// Models/Article.cs - تحديث النموذج مع إضافة علاقة Identity
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ArticlProject.Models
{
    public class Article
    {
        [Key]
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "العنوان مطلوب")]
        [Display(Name = "عنوان المقال")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "المحتوى مطلوب")]
        [Display(Name = "محتوى المقال")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "صورة المقال")]
        public string Image { get; set; } = string.Empty;

        [NotMapped]
        [Display(Name = "رفع صورة")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "النوع مطلوب")]
        [Display(Name = "نوع المقال")]
        public string Type { get; set; } = string.Empty;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // الحفاظ على العلاقة مع Author مع إضافة UserId
        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        // إضافة علاقة مع Identity
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }
    }
}

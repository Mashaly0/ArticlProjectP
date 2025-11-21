// Data/SeedData.cs
using Microsoft.AspNetCore.Identity;
using ArticlProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ArticlProject.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // إنشاء الأدوار
            string[] roleNames = { "Admin", "User", "Author" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // إنشاء مستخدم مسؤول
            var adminUser = new IdentityUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true
            };

            string adminPassword = "Admin123!";
            var user = await userManager.FindByEmailAsync(adminUser.Email);

            if (user == null)
            {
                var createPowerUser = await userManager.CreateAsync(adminUser, adminPassword);
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // إضافة بيانات نموذجية للمؤلفين إذا لم تكن موجودة
            if (!context.Authors.Any())
            {
                context.Authors.AddRange(
                    new Author
                    {
                        Name = "أحمد محمد",
                        About = "كاتب ومحرر متخصص في التقنية والبرمجة",
                        Image = ""
                    },
                    new Author
                    {
                        Name = "فاطمة عبدالله",
                        About = "كاتبة متخصصة في الأدب واللغة العربية",
                        Image = ""
                    }
                );
                await context.SaveChangesAsync();
            }

            // إضافة مقالات نموذجية إذا لم تكن موجودة
            if (!context.Articles.Any())
            {
                var author = await context.Authors.FirstAsync();
                context.Articles.AddRange(
                    new Article
                    {
                        Title = "مقدمة في ASP.NET Core",
                        Description = "هذه مقالة تمهيدية عن ASP.NET Core وإطار العمل MVC...",
                        Type = "تقنية",
                        CreatedDate = DateTime.Now,
                        AuthorId = author.AuthorId,
                        Image = ""
                    },
                    new Article
                    {
                        Title = "أساسيات البرمجة كائنية التوجه",
                        Description = "شرح للمفاهيم الأساسية في البرمجة كائنية التوجه...",
                        Type = "برمجة",
                        CreatedDate = DateTime.Now.AddDays(-1),
                        AuthorId = author.AuthorId,
                        Image = ""
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
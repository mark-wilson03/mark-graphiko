using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MarkGraphiko.Models;

namespace MarkGraphiko.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database tables exist
        await context.Database.EnsureCreatedAsync();

        // Safe column synchronization for SQLite
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE Messages ADD COLUMN IsArchived INTEGER NOT NULL DEFAULT 0;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE Messages ADD COLUMN ArchivedAt TEXT NULL;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE Messages ADD COLUMN IsDeletedByUser INTEGER NOT NULL DEFAULT 0;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE Messages ADD COLUMN IsDeletedByAdmin INTEGER NOT NULL DEFAULT 0;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE AspNetUsers ADD COLUMN PasswordResetCode TEXT NULL;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE AspNetUsers ADD COLUMN PasswordResetCodeExpiry TEXT NULL;"); } catch { }

        // 1. Seed Roles
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Admin User (Mark Wilson Allado)
        string adminEmail = "markwilsonquintoallado@gmail.com";
        string adminPassword = "markwilson123";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Mark Wilson Allado",
                Bio = "Creative Lead & Apparel Designer at MARK GRAPHIKO. Specializing in full-sublimation sport/club polo uniforms, streetwear graphics, brand logos, and vector illustrations.",
                AvatarUrl = "/images/mark-profile.jpg",
                RegistrationProvider = "Local",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            // Ensure password is set to requested markwilson123
            if (await userManager.HasPasswordAsync(adminUser))
            {
                await userManager.RemovePasswordAsync(adminUser);
            }
            await userManager.AddPasswordAsync(adminUser, adminPassword);

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            adminUser.FullName = "Mark Wilson Allado";
            adminUser.AvatarUrl = "/images/mark-profile.jpg";
            adminUser.EmailConfirmed = true;
            await userManager.UpdateAsync(adminUser);
        }

        // 4. Seed Categories
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { Name = "T-Shirt & Sublimation Mockups", Slug = "t-shirt-mockup", Description = "Full-sublimation polo uniforms, apparel mockups, oversized streetwear tees, and hoodie prints.", DisplayOrder = 1 },
                new Category { Name = "Logo Design", Slug = "logo-design", Description = "Minimalist brandmarks, emblems, modern badges, and luxury insignias.", DisplayOrder = 2 },
                new Category { Name = "Vector Art", Slug = "vector-art", Description = "Scalable high-precision illustrations, cyberpunk characters, and decals.", DisplayOrder = 3 },
                new Category { Name = "Branding & Identity", Slug = "branding-identity", Description = "Comprehensive brand systems, stationery, typography rules, and guides.", DisplayOrder = 4 },
                new Category { Name = "Typography", Slug = "typography", Description = "Expressive custom lettering, brutalist posters, and editorial font art.", DisplayOrder = 5 }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}

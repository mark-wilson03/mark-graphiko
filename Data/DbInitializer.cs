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
        string adminEmail = "admin@markgraphiko.com";
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

            var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            // Update to official name & avatar
            adminUser.FullName = "Mark Wilson Allado";
            adminUser.AvatarUrl = "/images/mark-profile.jpg";
            await userManager.UpdateAsync(adminUser);
        }

        // 3. Seed Sample Client User
        string clientEmail = "client@nexusstudio.io";
        var clientUser = await userManager.FindByEmailAsync(clientEmail);
        if (clientUser == null)
        {
            clientUser = new ApplicationUser
            {
                UserName = clientEmail,
                Email = clientEmail,
                FullName = "Elena Vance",
                Bio = "Creative Director at Nexus Studio.",
                AvatarUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=400&q=80",
                RegistrationProvider = "Google",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };

            var createResult = await userManager.CreateAsync(clientUser, "Client123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(clientUser, "User");
            }
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

        // 5. Ensure Flagship Sublimation Shirt Design is Seeded
        var catTShirt = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "t-shirt-mockup") ?? await context.Categories.FirstAsync();
        var catLogo = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "logo-design") ?? catTShirt;
        var catVector = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "vector-art") ?? catTShirt;
        var catBranding = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "branding-identity") ?? catTShirt;

        var flagshipDesign = await context.DesignPosts.FirstOrDefaultAsync(d => d.Title.Contains("SFISSC"));
        if (flagshipDesign == null)
        {
            var sfissc = new DesignPost
            {
                Title = "SFISSC SCIENCE CLUB UNIFORM | Sublimation Polo Shirt",
                Description = "Custom full-sublimation polo shirt design engineered in Adobe Photoshop by Mark Wilson Allado. Features a gradient electric purple DNA double helix motif, mechanical gear vector graphics, printed collar and button placket, and customized typography using MicroSport, Ease Bold, and Nulshock fonts.",
                CategoryId = catTShirt.Id,
                ImagePath = "/images/sfissc-sublimation-shirt.jpg",
                Client = "SFISSC Science Organization",
                ToolsUsed = "Adobe Photoshop, Illustrator",
                Tags = "sublimation, polo-shirt, apparel, science-club, vector, typography",
                IsFeatured = true,
                ViewCount = 4520,
                CreatedAt = DateTime.UtcNow
            };

            context.DesignPosts.Add(sfissc);
            await context.SaveChangesAsync();
        }

        // Check if Mark Graphiko Official Logo Brandmark is in DesignPosts
        var logoDesign = await context.DesignPosts.FirstOrDefaultAsync(d => d.Title.Contains("MARK GRAPHIKO Official Logo"));
        if (logoDesign == null)
        {
            var mgLogo = new DesignPost
            {
                Title = "MARK GRAPHIKO Official Logo | Hexagonal Brandmark",
                Description = "Official studio brandmark embodying 'Where Art Meets Design'. Features an interlocking geometric M-G monogram enclosed within a modern hexagon badge, accented in ivory white and deep industrial amber.",
                CategoryId = catLogo.Id,
                ImagePath = "/images/mark-graphiko-logo.png",
                Client = "Mark Graphiko Studio",
                ToolsUsed = "Adobe Illustrator, Photoshop",
                Tags = "logo, brandmark, monogram, geometric, emblem",
                IsFeatured = true,
                ViewCount = 3890,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            context.DesignPosts.Add(mgLogo);
            await context.SaveChangesAsync();
        }
    }
}

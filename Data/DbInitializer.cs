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

        // 6. Seed Additional Featured Portfolio Works for Full Carousel Showcase
        var jerseyDesign = await context.DesignPosts.FirstOrDefaultAsync(d => d.Title.Contains("NEON APEX MOTORSPORTS"));
        if (jerseyDesign == null)
        {
            context.DesignPosts.Add(new DesignPost
            {
                Title = "NEON APEX MOTORSPORTS | Full-Sublimation Racing Jersey",
                Description = "Dynamic esports and motorsports team jersey featuring angular neon cyan and blazing orange kinetic vector elements, contrast trim collar, and aerodynamic team sponsor layout.",
                CategoryId = catTShirt.Id,
                ImagePath = "/images/neon-apex-jersey.jpg",
                Client = "Neon Apex Racing Team",
                ToolsUsed = "Adobe Photoshop, Illustrator",
                Tags = "sublimation, jersey, motorsports, racing, apparel, neon",
                IsFeatured = true,
                ViewCount = 5240,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            });
        }

        var samuraiDesign = await context.DesignPosts.FirstOrDefaultAsync(d => d.Title.Contains("CYBERPUNK ONI SAMURAI"));
        if (samuraiDesign == null)
        {
            context.DesignPosts.Add(new DesignPost
            {
                Title = "CYBERPUNK ONI SAMURAI | Vector Character Illustration",
                Description = "Master vector illustration blending traditional Japanese samurai armor with cybernetic visor tech, glowing neon katana blade, and intricate apparel decals.",
                CategoryId = catVector.Id,
                ImagePath = "/images/cyberpunk-oni-samurai.jpg",
                Client = "Rebel Studio Tokyo",
                ToolsUsed = "Adobe Illustrator",
                Tags = "vector-art, cyberpunk, samurai, oni, illustration, streetwear",
                IsFeatured = true,
                ViewCount = 6180,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            });
        }

        var valkyrieDesign = await context.DesignPosts.FirstOrDefaultAsync(d => d.Title.Contains("VALKYRIE ATHLETIC EXCELLENCE"));
        if (valkyrieDesign == null)
        {
            context.DesignPosts.Add(new DesignPost
            {
                Title = "VALKYRIE ATHLETIC EXCELLENCE | Custom Team Polo",
                Description = "High-performance collegiate athletic polo shirt engineered with interlocking gold and navy blue polygonal geometric vectors, button placket, and crest embroidery detailing.",
                CategoryId = catTShirt.Id,
                ImagePath = "/images/valkyrie-athletic-polo.jpg",
                Client = "Valkyrie Athletics League",
                ToolsUsed = "Adobe Photoshop, Illustrator",
                Tags = "polo-shirt, sublimation, athletic, apparel, geometric, luxury",
                IsFeatured = true,
                ViewCount = 4890,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            });
        }

        var brutalistDesign = await context.DesignPosts.FirstOrDefaultAsync(d => d.Title.Contains("BRUTALIST ARCHITECT"));
        if (brutalistDesign == null)
        {
            context.DesignPosts.Add(new DesignPost
            {
                Title = "BRUTALIST ARCHITECT | Streetwear Collective Poster",
                Description = "Experimental brutalist typography and oversized apparel poster print showcasing distressed textures, warped gothic typefaces, and industrial urban architecture motifs.",
                CategoryId = catBranding.Id,
                ImagePath = "/images/brutalist-streetwear-poster.jpg",
                Client = "Urban Decay Collective",
                ToolsUsed = "Adobe Photoshop, InDesign",
                Tags = "typography, brutalist, streetwear, poster, branding, raw-type",
                IsFeatured = true,
                ViewCount = 3940,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            });
        }

        await context.SaveChangesAsync();
    }
}

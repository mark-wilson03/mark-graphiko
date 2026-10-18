using Microsoft.EntityFrameworkCore;
using MarkGraphiko.Data;
using MarkGraphiko.Models;
using MarkGraphiko.Models.ViewModels;

namespace MarkGraphiko.Services;

public class DesignService : IDesignService
{
    private readonly ApplicationDbContext _context;

    public DesignService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DesignPost>> GetFeaturedDesignsAsync()
    {
        return await _context.DesignPosts
            .Include(d => d.Category)
            .Where(d => d.IsFeatured)
            .OrderByDescending(d => d.CreatedAt)
            .Take(8)
            .ToListAsync();
    }

    public async Task<List<DesignPost>> GetRecentDesignsAsync(int count = 6)
    {
        return await _context.DesignPosts
            .Include(d => d.Category)
            .OrderByDescending(d => d.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<DesignPost>> GetDesignsFilteredAsync(string? categorySlug, string? searchQuery)
    {
        var query = _context.DesignPosts
            .Include(d => d.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(categorySlug) && categorySlug.ToLower() != "all")
        {
            query = query.Where(d => d.Category != null && d.Category.Slug.ToLower() == categorySlug.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var term = searchQuery.Trim().ToLower();
            query = query.Where(d =>
                d.Title.ToLower().Contains(term) ||
                d.Description.ToLower().Contains(term) ||
                (d.Tags != null && d.Tags.ToLower().Contains(term)) ||
                (d.Client != null && d.Client.ToLower().Contains(term)) ||
                (d.Category != null && d.Category.Name.ToLower().Contains(term)));
        }

        return await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
    }

    public async Task<DesignPost?> GetDesignByIdAsync(int id)
    {
        return await _context.DesignPosts
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DesignPost> CreateDesignAsync(UploadDesignViewModel model, string imagePath)
    {
        var design = new DesignPost
        {
            Title = model.Title.Trim(),
            CategoryId = model.CategoryId,
            Description = model.Description?.Trim() ?? string.Empty,
            ImagePath = imagePath,
            Client = model.Client?.Trim(),
            ToolsUsed = model.ToolsUsed?.Trim(),
            Tags = model.Tags?.Trim(),
            IsFeatured = model.IsFeatured,
            CreatedAt = DateTime.UtcNow
        };

        _context.DesignPosts.Add(design);
        await _context.SaveChangesAsync();
        return design;
    }

    public async Task<bool> UpdateDesignAsync(UploadDesignViewModel model, string? newImagePath)
    {
        if (!model.Id.HasValue) return false;

        var design = await _context.DesignPosts.FindAsync(model.Id.Value);
        if (design == null) return false;

        design.Title = model.Title.Trim();
        design.CategoryId = model.CategoryId;
        design.Description = model.Description?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(newImagePath))
        {
            design.ImagePath = newImagePath;
        }
        design.Client = model.Client?.Trim();
        design.ToolsUsed = model.ToolsUsed?.Trim();
        design.Tags = model.Tags?.Trim();
        design.IsFeatured = model.IsFeatured;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDesignAsync(int id)
    {
        var design = await _context.DesignPosts.FindAsync(id);
        if (design == null) return false;

        _context.DesignPosts.Remove(design);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task IncrementViewCountAsync(int id)
    {
        var design = await _context.DesignPosts.FindAsync(id);
        if (design != null)
        {
            design.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ToggleFavoriteAsync(string userId, int designId)
    {
        var existing = await _context.FavoriteDesigns
            .FirstOrDefaultAsync(f => f.UserId == userId && f.DesignPostId == designId);

        if (existing != null)
        {
            _context.FavoriteDesigns.Remove(existing);
            await _context.SaveChangesAsync();
            return false; // Removed
        }
        else
        {
            _context.FavoriteDesigns.Add(new FavoriteDesign
            {
                UserId = userId,
                DesignPostId = designId,
                FavoritedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true; // Added
        }
    }

    public async Task<List<DesignPost>> GetUserFavoritesAsync(string userId)
    {
        return await _context.FavoriteDesigns
            .Include(f => f.DesignPost)
            .ThenInclude(d => d!.Category)
            .Where(f => f.UserId == userId && f.DesignPost != null)
            .OrderByDescending(f => f.FavoritedAt)
            .Select(f => f.DesignPost!)
            .ToListAsync();
    }

    public async Task<HashSet<int>> GetUserFavoriteIdsAsync(string userId)
    {
        var ids = await _context.FavoriteDesigns
            .Where(f => f.UserId == userId)
            .Select(f => f.DesignPostId)
            .ToListAsync();

        return new HashSet<int>(ids);
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }
}

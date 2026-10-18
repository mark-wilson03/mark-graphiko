using MarkGraphiko.Models;
using MarkGraphiko.Models.ViewModels;

namespace MarkGraphiko.Services;

public interface IDesignService
{
    Task<List<DesignPost>> GetFeaturedDesignsAsync();
    Task<List<DesignPost>> GetRecentDesignsAsync(int count = 6);
    Task<List<DesignPost>> GetDesignsFilteredAsync(string? categorySlug, string? searchQuery);
    Task<DesignPost?> GetDesignByIdAsync(int id);
    Task<DesignPost> CreateDesignAsync(UploadDesignViewModel model, string imagePath);
    Task<bool> UpdateDesignAsync(UploadDesignViewModel model, string? newImagePath);
    Task<bool> DeleteDesignAsync(int id);
    Task IncrementViewCountAsync(int id);
    Task<bool> ToggleFavoriteAsync(string userId, int designId);
    Task<List<DesignPost>> GetUserFavoritesAsync(string userId);
    Task<HashSet<int>> GetUserFavoriteIdsAsync(string userId);
    Task<List<Category>> GetCategoriesAsync();
}

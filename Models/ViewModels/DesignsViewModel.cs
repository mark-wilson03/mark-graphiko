using MarkGraphiko.Models;

namespace MarkGraphiko.Models.ViewModels;

public class DesignsViewModel
{
    public List<DesignPost> Designs { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public string? SelectedCategorySlug { get; set; }
    public string? SearchQuery { get; set; }
    public HashSet<int> UserFavoriteDesignIds { get; set; } = new();
    public int TotalCount { get; set; }
}

using MarkGraphiko.Models;

namespace MarkGraphiko.Models.ViewModels;

public class HomeViewModel
{
    public List<DesignPost> FeaturedDesigns { get; set; } = new();
    public List<DesignPost> RecentWorks { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public int TotalDesignsCount { get; set; }
    public int HappyClientsCount { get; set; } = 84;
    public int YearsExperience { get; set; } = 6;
}

using MarkGraphiko.Models;

namespace MarkGraphiko.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalDesigns { get; set; }
    public int TotalCategories { get; set; }
    public int TotalInquiries { get; set; }
    public int UnreadInquiries { get; set; }
    public int TotalUsers { get; set; }
    public List<DesignPost> RecentDesigns { get; set; } = new();
    public List<Message> RecentMessages { get; set; } = new();
    public List<CategoryStat> CategoryStats { get; set; } = new();
}

public class CategoryStat
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AdminReplyInputModel
{
    public int MessageId { get; set; }
    public string ReplyContent { get; set; } = string.Empty;
}

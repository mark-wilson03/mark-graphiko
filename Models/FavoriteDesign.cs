using System.ComponentModel.DataAnnotations;

namespace MarkGraphiko.Models;

public class FavoriteDesign
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser? User { get; set; }

    [Required]
    public int DesignPostId { get; set; }
    public virtual DesignPost? DesignPost { get; set; }

    public DateTime FavoritedAt { get; set; } = DateTime.UtcNow;
}

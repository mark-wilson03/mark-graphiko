using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarkGraphiko.Models;

public class DesignPost
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ThumbnailPath { get; set; }

    [MaxLength(150)]
    public string? Client { get; set; }

    [MaxLength(250)]
    public string? ToolsUsed { get; set; } // e.g., "Photoshop, Illustrator, Figma"

    [MaxLength(300)]
    public string? Tags { get; set; } // e.g., "tshirt, apparel, streetwear, vector"

    public bool IsFeatured { get; set; } = false;

    public int ViewCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<FavoriteDesign> FavoritedBy { get; set; } = new List<FavoriteDesign>();
}

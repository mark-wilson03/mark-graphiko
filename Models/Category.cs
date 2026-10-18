using System.ComponentModel.DataAnnotations;

namespace MarkGraphiko.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Description { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    public virtual ICollection<DesignPost> DesignPosts { get; set; } = new List<DesignPost>();
}

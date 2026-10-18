using System.ComponentModel.DataAnnotations;

namespace MarkGraphiko.Models;

public class Message
{
    public int Id { get; set; }

    // Nullable if user sent message or connected account
    public string? SenderId { get; set; }
    public virtual ApplicationUser? Sender { get; set; }

    [Required]
    [MaxLength(150)]
    public string SenderName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string SenderEmail { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ProjectType { get; set; } = "T-Shirt Mockup"; // T-Shirt Mockup, Logo Design, Vector Art, Brand Identity, Other

    [MaxLength(50)]
    public string BudgetRange { get; set; } = "$100 - $300";

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;

    public bool IsArchived { get; set; } = false;

    public DateTime? ArchivedAt { get; set; }

    public bool IsDeletedByUser { get; set; } = false;

    public bool IsDeletedByAdmin { get; set; } = false;

    [MaxLength(4000)]
    public string? AdminReply { get; set; }

    public DateTime? RepliedAt { get; set; }
}

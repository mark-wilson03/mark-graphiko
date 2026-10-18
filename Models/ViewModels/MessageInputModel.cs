using System.ComponentModel.DataAnnotations;

namespace MarkGraphiko.Models.ViewModels;

public class MessageInputModel
{
    [Required(ErrorMessage = "Please provide your full name.")]
    [MaxLength(100)]
    public string SenderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide your email address.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(150)]
    public string SenderEmail { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Please select a project type.")]
    public string ProjectType { get; set; } = "T-Shirt Mockup & Apparel";

    [Required(ErrorMessage = "Please select an estimated budget.")]
    public string BudgetRange { get; set; } = "$100 - $300";

    [Required(ErrorMessage = "Subject line is required.")]
    [MaxLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide details regarding your design needs or timeline.")]
    [MaxLength(3000)]
    public string Content { get; set; } = string.Empty;

    public int? DesignReferenceId { get; set; }
}

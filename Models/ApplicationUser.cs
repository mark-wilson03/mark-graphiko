using Microsoft.AspNetCore.Identity;

namespace MarkGraphiko.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string RegistrationProvider { get; set; } = "Local"; // Local, Google, Facebook
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Password modification verification code
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiry { get; set; }

    // Navigation properties
    public virtual ICollection<FavoriteDesign> Favorites { get; set; } = new List<FavoriteDesign>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

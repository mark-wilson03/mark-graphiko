using MarkGraphiko.Models;

namespace MarkGraphiko.Models.ViewModels;

public class ProfileViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public List<DesignPost> SavedFavorites { get; set; } = new();
    public List<Message> ActiveInquiries { get; set; } = new();
    public List<Message> ArchivedInquiries { get; set; } = new();
    public List<Message> MyInquiries { get => ActiveInquiries; set => ActiveInquiries = value; }
    public string? StatusMessage { get; set; }
}

public class UpdateProfileInputModel
{
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
}

public class ChangePasswordInputModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "6-digit verification code is required")]
    [System.ComponentModel.DataAnnotations.StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be exactly 6 digits")]
    public string VerificationCode { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "New password is required")]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}


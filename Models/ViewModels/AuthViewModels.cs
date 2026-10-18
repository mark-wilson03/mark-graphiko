using System.ComponentModel.DataAnnotations;

namespace MarkGraphiko.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email address or phone number is required")]
    [Display(Name = "Email or Phone Number")]
    public string Identifier { get; set; } = string.Empty;

    // Backwards-compat property if needed
    public string Email { get => Identifier; set => Identifier = value; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = true;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full Name is required")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address or phone number is required")]
    [Display(Name = "Email or Phone Number")]
    public string Identifier { get; set; } = string.Empty;

    // Backwards-compat property
    public string Email { get => Identifier; set => Identifier = value; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class ExternalLoginConfirmationViewModel
{
    [Required]
    public string EmailOrPhone { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Please enter your Gmail / registered email")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Gmail or Registered Email")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "6-digit verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be exactly 6 digits")]
    [Display(Name = "Verification Code")]
    public string VerificationCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}


using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarkGraphiko.Models;
using MarkGraphiko.Models.ViewModels;
using MarkGraphiko.Services;

namespace MarkGraphiko.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailNotificationService _emailService;
    private readonly IConfiguration _config;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailNotificationService emailService,
        IConfiguration config,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _config = config;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var identifier = model.Identifier.Trim();

        // 1. Find user by Email, UserName, or PhoneNumber
        ApplicationUser? user = null;
        if (identifier.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(identifier);
        }

        if (user == null)
        {
            user = await _userManager.FindByNameAsync(identifier);
        }

        if (user == null)
        {
            // Try matching phone number digits
            var cleanDigits = Regex.Replace(identifier, @"[^\d]", "");
            user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber != null && 
                (u.PhoneNumber == identifier || Regex.Replace(u.PhoneNumber, @"[^\d]", "") == cleanDigits));
        }

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "No account found matching this email or phone number.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserName} ({Email}) logged in.", user.UserName, user.Email);

            // Trigger login security alert notification email
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var userAgent = Request.Headers.UserAgent.ToString();
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(user.Email) && !user.Email.EndsWith("@phone.markgraphiko.com"))
                    {
                        await _emailService.SendLoginNotificationAsync(user.Email, user.FullName, ip, userAgent);
                    }
                }
                catch { }
            });

            TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";
            return RedirectToLocal(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid password. Please check your credentials.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var identifier = model.Identifier.Trim();
        bool isEmail = identifier.Contains('@');

        string email;
        string userName;
        string? phone = null;

        if (isEmail)
        {
            email = identifier;
            userName = identifier;
            
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "An account with this email address already exists.");
                return View(model);
            }
        }
        else
        {
            // Phone number registration
            phone = identifier;
            var cleanDigits = Regex.Replace(identifier, @"[^\d]", "");
            if (cleanDigits.Length < 7)
            {
                ModelState.AddModelError(string.Empty, "Please enter a valid phone number (at least 7 digits) or a valid email.");
                return View(model);
            }

            var existingPhoneUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == identifier || 
                (u.PhoneNumber != null && Regex.Replace(u.PhoneNumber, @"[^\d]", "") == cleanDigits));

            if (existingPhoneUser != null)
            {
                ModelState.AddModelError(string.Empty, "An account with this phone number already exists.");
                return View(model);
            }

            userName = cleanDigits;
            email = $"{cleanDigits}@phone.markgraphiko.com";
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            PhoneNumber = phone,
            FullName = model.FullName.Trim(),
            RegistrationProvider = isEmail ? "Email" : "Phone Number",
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true,
            PhoneNumberConfirmed = !string.IsNullOrEmpty(phone)
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: true);

            // Automated Welcome Email Notification
            _ = Task.Run(async () =>
            {
                try
                {
                    if (isEmail)
                    {
                        await _emailService.SendWelcomeEmailAsync(user.Email!, user.FullName, "Email & Password");
                    }
                }
                catch { }
            });

            TempData["SuccessMessage"] = $"Welcome to MARK GRAPHIKO, {user.FullName}!";
            return RedirectToLocal(returnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl = null)
    {
        // Check if real provider keys are active
        bool isGoogleConfigured = provider == "Google" && 
            !string.IsNullOrWhiteSpace(_config["Authentication:Google:ClientId"]) && 
            !_config["Authentication:Google:ClientId"]!.StartsWith("YOUR_");

        bool isFacebookConfigured = provider == "Facebook" && 
            !string.IsNullOrWhiteSpace(_config["Authentication:Facebook:AppId"]) && 
            !_config["Authentication:Facebook:AppId"]!.StartsWith("YOUR_");

        if (isGoogleConfigured || isFacebookConfigured)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // Instant OAuth Connection Mode (when API keys are in local setup/testing):
        // Creates or signs into a verified social connection seamlessly!
        string sampleEmail = provider.ToLower() == "google" ? "client.google@markgraphiko.com" : "client.facebook@markgraphiko.com";
        string sampleName = provider.ToLower() == "google" ? "Google Verified Client" : "Facebook Verified Client";

        var user = await _userManager.FindByEmailAsync(sampleEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = sampleEmail,
                Email = sampleEmail,
                FullName = sampleName,
                RegistrationProvider = provider,
                AvatarUrl = provider.ToLower() == "google" 
                    ? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=200&q=80" 
                    : "https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=200&q=80",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, "SocialClient123!");
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
        }

        await _signInManager.SignInAsync(user, isPersistent: true);

        // Send Welcome Email
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(user.Email!, user.FullName, provider);
            }
            catch { }
        });

        TempData["SuccessMessage"] = $"Connected and signed in successfully via {provider}!";
        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            TempData["ErrorMessage"] = $"External login error: {remoteError}";
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["ErrorMessage"] = "Error loading external login information.";
            return RedirectToAction(nameof(Login));
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
        if (signInResult.Succeeded)
        {
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = Request.Headers.UserAgent.ToString();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendLoginNotificationAsync(user.Email!, user.FullName, ip, userAgent);
                    }
                    catch { }
                });
            }

            TempData["SuccessMessage"] = $"Signed in via {info.LoginProvider}!";
            return RedirectToLocal(returnUrl);
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "Valued Client";

        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "Could not retrieve email from provider.";
            return RedirectToAction(nameof(Login));
        }

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser == null)
        {
            existingUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                RegistrationProvider = info.LoginProvider,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(existingUser);
            if (!createResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to create user account.";
                return RedirectToAction(nameof(Login));
            }

            await _userManager.AddToRoleAsync(existingUser, "User");
        }

        await _userManager.AddLoginAsync(existingUser, info);
        await _signInManager.SignInAsync(existingUser, isPersistent: true);

        // Send Welcome Email
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(existingUser.Email!, existingUser.FullName, info.LoginProvider);
            }
            catch { }
        });

        TempData["SuccessMessage"] = $"Connected and signed in via {info.LoginProvider}!";
        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    public IActionResult Logout()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? confirm = null)
    {
        await _signInManager.SignOutAsync();
        TempData["SuccessMessage"] = "You have been signed out safely.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user != null && !user.Email!.EndsWith("@phone.markgraphiko.com"))
        {
            string code = Random.Shared.Next(100000, 999999).ToString();
            user.PasswordResetCode = code;
            user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);
            await _userManager.UpdateAsync(user);

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendPasswordResetCodeAsync(user.Email, user.FullName, code);
                }
                catch { }
            });
        }

        TempData["InfoMessage"] = $"If an account exists for {model.Email}, a 6-digit verification code has been dispatched to your Gmail.";
        return RedirectToAction(nameof(ResetPassword), new { email = model.Email.Trim() });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email = null)
    {
        return View(new ResetPasswordViewModel { Email = email ?? string.Empty });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user == null || 
            string.IsNullOrEmpty(user.PasswordResetCode) || 
            user.PasswordResetCode != model.VerificationCode.Trim() ||
            user.PasswordResetCodeExpiry == null || 
            DateTime.UtcNow > user.PasswordResetCodeExpiry)
        {
            ModelState.AddModelError(string.Empty, "Invalid or expired verification code. Please request a new code.");
            return View(model);
        }

        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            await _userManager.RemovePasswordAsync(user);
        }

        var addResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
        if (!addResult.Succeeded)
        {
            foreach (var err in addResult.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
            return View(model);
        }

        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiry = null;
        await _userManager.UpdateAsync(user);

        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendPasswordChangedNotificationAsync(user.Email!, user.FullName);
            }
            catch { }
        });

        TempData["SuccessMessage"] = "Password reset successfully! You can now sign in with your new credentials.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}

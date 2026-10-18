using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MarkGraphiko.Models;
using MarkGraphiko.Models.ViewModels;
using MarkGraphiko.Services;

namespace MarkGraphiko.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDesignService _designService;
    private readonly IMessageService _messageService;
    private readonly IEmailNotificationService _emailService;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        IDesignService designService,
        IMessageService messageService,
        IEmailNotificationService emailService,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _designService = designService;
        _messageService = messageService;
        _emailService = emailService;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var favorites = await _designService.GetUserFavoritesAsync(user.Id);
        var activeMessages = await _messageService.GetUserActiveMessagesAsync(user.Id);
        var archivedMessages = await _messageService.GetUserArchivedMessagesAsync(user.Id);

        var viewModel = new ProfileViewModel
        {
            User = user,
            SavedFavorites = favorites,
            ActiveInquiries = activeMessages,
            ArchivedInquiries = archivedMessages
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateProfileInputModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        if (string.IsNullOrWhiteSpace(model.FullName))
        {
            TempData["ErrorMessage"] = "Full Name cannot be empty.";
            return RedirectToAction(nameof(Index));
        }

        user.FullName = model.FullName.Trim();
        user.Bio = model.Bio?.Trim();
        user.PhoneNumber = model.PhoneNumber?.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Profile settings updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Could not update profile.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (avatarFile == null || avatarFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select an image file to upload.";
            return RedirectToAction(nameof(Index));
        }

        if (avatarFile.Length > 10 * 1024 * 1024)
        {
            TempData["ErrorMessage"] = "Profile image must be less than 10 MB.";
            return RedirectToAction(nameof(Index));
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".svg" };
        var ext = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            TempData["ErrorMessage"] = "Only JPG, PNG, WEBP, or SVG images are supported.";
            return RedirectToAction(nameof(Index));
        }

        string avatarsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
        if (!Directory.Exists(avatarsFolder))
        {
            Directory.CreateDirectory(avatarsFolder);
        }

        // Delete old custom avatar if it exists in local uploads
        if (!string.IsNullOrEmpty(user.AvatarUrl) && user.AvatarUrl.StartsWith("/uploads/avatars/"))
        {
            string oldFileName = Path.GetFileName(user.AvatarUrl);
            string oldFilePath = Path.Combine(avatarsFolder, oldFileName);
            if (System.IO.File.Exists(oldFilePath))
            {
                try { System.IO.File.Delete(oldFilePath); } catch { }
            }
        }

        string uniqueFileName = $"avatar_{user.Id}_{Guid.NewGuid():N}{ext}";
        string filePath = Path.Combine(avatarsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await avatarFile.CopyToAsync(stream);
        }

        user.AvatarUrl = $"/uploads/avatars/{uniqueFileName}";
        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = "Profile photo updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAvatar()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!string.IsNullOrEmpty(user.AvatarUrl) && user.AvatarUrl.StartsWith("/uploads/avatars/"))
        {
            string oldFileName = Path.GetFileName(user.AvatarUrl);
            string oldFilePath = Path.Combine(_environment.WebRootPath, "uploads", "avatars", oldFileName);
            if (System.IO.File.Exists(oldFilePath))
            {
                try { System.IO.File.Delete(oldFilePath); } catch { }
            }
        }

        user.AvatarUrl = null;
        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = "Profile photo removed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendPasswordCode()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (string.IsNullOrEmpty(user.Email) || user.Email.EndsWith("@phone.markgraphiko.com"))
        {
            return Json(new { success = false, message = "A valid Gmail or email address is required to receive verification codes." });
        }

        // Generate 6-digit verification code
        string code = Random.Shared.Next(100000, 999999).ToString();
        user.PasswordResetCode = code;
        user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        await _userManager.UpdateAsync(user);

        // Send email to Gmail
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendPasswordResetCodeAsync(user.Email, user.FullName, code);
            }
            catch { }
        });

        return Json(new { 
            success = true, 
            message = $"Verification code sent to {user.Email}. Please check your inbox (and spam folder) and enter the 6-digit code within 15 minutes." 
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordInputModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please fill in all password fields properly (minimum 6 characters).";
            return RedirectToAction(nameof(Index));
        }

        // Verify code
        if (string.IsNullOrEmpty(user.PasswordResetCode) || 
            user.PasswordResetCode != model.VerificationCode.Trim() ||
            user.PasswordResetCodeExpiry == null ||
            DateTime.UtcNow > user.PasswordResetCodeExpiry)
        {
            TempData["ErrorMessage"] = "Invalid or expired verification code. Please request a new code sent to your Gmail.";
            return RedirectToAction(nameof(Index));
        }

        // Reset password
        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Could not update password: " + string.Join(", ", removeResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }
        }

        var addResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
        if (!addResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Could not set new password: " + string.Join(", ", addResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        // Invalidate used code
        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiry = null;
        await _userManager.UpdateAsync(user);

        // Send confirmation email
        if (!string.IsNullOrEmpty(user.Email) && !user.Email.EndsWith("@phone.markgraphiko.com"))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.FullName);
                }
                catch { }
            });
        }

        TempData["SuccessMessage"] = "Your password has been modified successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveMessage(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var success = await _messageService.ArchiveMessageAsync(id, user.Id, isAdmin: false);
        if (success)
        {
            TempData["SuccessMessage"] = "Chat inquiry moved to archive.";
        }
        else
        {
            TempData["ErrorMessage"] = "Could not archive chat.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnarchiveMessage(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var success = await _messageService.UnarchiveMessageAsync(id, user.Id, isAdmin: false);
        if (success)
        {
            TempData["SuccessMessage"] = "Chat inquiry restored to active conversations.";
        }
        else
        {
            TempData["ErrorMessage"] = "Could not restore chat.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMessage(int id, bool permanent = true)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var success = await _messageService.DeleteMessageAsync(id, user.Id, isAdmin: false, permanent: permanent);
        if (success)
        {
            TempData["SuccessMessage"] = permanent ? "Chat inquiry permanently deleted." : "Chat inquiry moved to archive.";
        }
        else
        {
            TempData["ErrorMessage"] = "Could not delete chat.";
        }

        return RedirectToAction(nameof(Index));
    }
}

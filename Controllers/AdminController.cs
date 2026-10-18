using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarkGraphiko.Data;
using MarkGraphiko.Models;
using MarkGraphiko.Models.ViewModels;
using MarkGraphiko.Services;

namespace MarkGraphiko.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDesignService _designService;
    private readonly IMessageService _messageService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public AdminController(
        ApplicationDbContext context,
        IDesignService designService,
        IMessageService messageService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment)
    {
        _context = context;
        _designService = designService;
        _messageService = messageService;
        _userManager = userManager;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var totalDesigns = await _context.DesignPosts.CountAsync();
        var totalCategories = await _context.Categories.CountAsync();
        var totalInquiries = await _context.Messages.CountAsync();
        var unreadInquiries = await _context.Messages.CountAsync(m => !m.IsRead);
        var totalUsers = await _userManager.Users.CountAsync();

        var recentDesigns = await _context.DesignPosts
            .Include(d => d.Category)
            .OrderByDescending(d => d.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentMessages = await _context.Messages
            .OrderByDescending(m => m.SentAt)
            .Take(5)
            .ToListAsync();

        var categoryStats = await _context.Categories
            .Select(c => new CategoryStat
            {
                Name = c.Name,
                Count = c.DesignPosts.Count
            })
            .ToListAsync();

        var model = new AdminDashboardViewModel
        {
            TotalDesigns = totalDesigns,
            TotalCategories = totalCategories,
            TotalInquiries = totalInquiries,
            UnreadInquiries = unreadInquiries,
            TotalUsers = totalUsers,
            RecentDesigns = recentDesigns,
            RecentMessages = recentMessages,
            CategoryStats = categoryStats
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Upload()
    {
        var categories = await _designService.GetCategoriesAsync();
        var model = new UploadDesignViewModel
        {
            CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(UploadDesignViewModel model)
    {
        if (model.ImageFile == null && string.IsNullOrWhiteSpace(model.ExistingImagePath))
        {
            ModelState.AddModelError("ImageFile", "Please upload a high-resolution image file or provide an image link.");
        }

        if (!ModelState.IsValid)
        {
            var categories = await _designService.GetCategoriesAsync();
            model.CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            return View(model);
        }

        string imagePath = model.ExistingImagePath ?? string.Empty;

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "designs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(model.ImageFile.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(fileStream);
            }

            imagePath = $"/uploads/designs/{uniqueFileName}";
        }

        var design = await _designService.CreateDesignAsync(model, imagePath);

        TempData["SuccessMessage"] = $"Design '{design.Title}' published successfully to the portfolio and carousels!";
        return RedirectToAction("Index", "Designs");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var design = await _designService.GetDesignByIdAsync(id);
        if (design == null) return NotFound();

        var categories = await _designService.GetCategoriesAsync();
        var model = new UploadDesignViewModel
        {
            Id = design.Id,
            Title = design.Title,
            CategoryId = design.CategoryId,
            Description = design.Description,
            ExistingImagePath = design.ImagePath,
            Client = design.Client,
            ToolsUsed = design.ToolsUsed,
            Tags = design.Tags,
            IsFeatured = design.IsFeatured,
            CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = c.Id == design.CategoryId
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UploadDesignViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _designService.GetCategoriesAsync();
            model.CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            return View(model);
        }

        string? newImagePath = null;
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "designs");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(model.ImageFile.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(fileStream);
            }

            newImagePath = $"/uploads/designs/{uniqueFileName}";
        }

        await _designService.UpdateDesignAsync(model, newImagePath);
        TempData["SuccessMessage"] = "Design updated successfully.";
        return RedirectToAction("Index", "Designs");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _designService.DeleteDesignAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Artwork removed from portfolio.";
        }
        else
        {
            TempData["ErrorMessage"] = "Could not delete artwork.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Messages(string? filter = "all")
    {
        var messages = filter switch
        {
            "unread" => await _messageService.GetUnreadMessagesAsync(),
            "archived" => await _messageService.GetAdminArchivedMessagesAsync(),
            _ => await _messageService.GetAllMessagesAsync()
        };

        ViewData["ActiveFilter"] = filter;
        return View(messages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _messageService.MarkAsReadAsync(id);
        TempData["SuccessMessage"] = "Inquiry marked as read.";
        return RedirectToAction(nameof(Messages));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveMessage(int id)
    {
        await _messageService.ArchiveMessageAsync(id, "", isAdmin: true);
        TempData["SuccessMessage"] = "Inquiry moved to archive.";
        return RedirectToAction(nameof(Messages));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnarchiveMessage(int id)
    {
        await _messageService.UnarchiveMessageAsync(id, "", isAdmin: true);
        TempData["SuccessMessage"] = "Inquiry restored to active inbox.";
        return RedirectToAction(nameof(Messages), new { filter = "archived" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        await _messageService.DeleteMessageAsync(id, "", isAdmin: true, permanent: true);
        TempData["SuccessMessage"] = "Inquiry permanently deleted.";
        return RedirectToAction(nameof(Messages));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(AdminReplyInputModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ReplyContent))
        {
            TempData["ErrorMessage"] = "Reply message cannot be empty.";
            return RedirectToAction(nameof(Messages));
        }

        var success = await _messageService.ReplyToMessageAsync(model.MessageId, model.ReplyContent);
        if (success)
        {
            TempData["SuccessMessage"] = "Reply dispatched to client's email.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to dispatch reply.";
        }

        return RedirectToAction(nameof(Messages));
    }
}

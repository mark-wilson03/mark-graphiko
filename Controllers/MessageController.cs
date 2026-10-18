using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MarkGraphiko.Models;
using MarkGraphiko.Models.ViewModels;
using MarkGraphiko.Services;

namespace MarkGraphiko.Controllers;

public class MessageController : Controller
{
    private readonly IMessageService _messageService;
    private readonly IDesignService _designService;
    private readonly UserManager<ApplicationUser> _userManager;

    public MessageController(
        IMessageService messageService,
        IDesignService designService,
        UserManager<ApplicationUser> userManager)
    {
        _messageService = messageService;
        _designService = designService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? designId = null)
    {
        var model = new MessageInputModel();

        if (designId.HasValue)
        {
            var design = await _designService.GetDesignByIdAsync(designId.Value);
            if (design != null)
            {
                model.DesignReferenceId = design.Id;
                model.Subject = $"Project Inquiry regarding '{design.Title}'";
                if (design.Category != null)
                {
                    model.ProjectType = design.Category.Name;
                }
            }
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                model.SenderName = user.FullName;
                model.SenderEmail = user.Email ?? string.Empty;
                model.PhoneNumber = user.PhoneNumber;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(MessageInputModel model)
    {
        // Enforce authentication rule
        if (User.Identity?.IsAuthenticated != true)
        {
            TempData["ErrorMessage"] = "Please sign up or log in before submitting a project inquiry.";
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Message") });
        }

        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var message = await _messageService.SendMessageAsync(model, userId);

        TempData["SuccessMessage"] = "Inquiry sent successfully! A confirmation receipt has been dispatched to your email, and Mark will review your brief shortly.";
        return RedirectToAction("Index", "Profile");
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarkGraphiko.Models.ViewModels;
using MarkGraphiko.Services;

namespace MarkGraphiko.Controllers;

public class DesignsController : Controller
{
    private readonly IDesignService _designService;

    public DesignsController(IDesignService designService)
    {
        _designService = designService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? category, string? q)
    {
        var designs = await _designService.GetDesignsFilteredAsync(category, q);
        var categories = await _designService.GetCategoriesAsync();

        var userFavoriteIds = new HashSet<int>();
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                userFavoriteIds = await _designService.GetUserFavoriteIdsAsync(userId);
            }
        }

        var viewModel = new DesignsViewModel
        {
            Designs = designs,
            Categories = categories,
            SelectedCategorySlug = string.IsNullOrWhiteSpace(category) ? "all" : category.ToLower(),
            SearchQuery = q,
            UserFavoriteDesignIds = userFavoriteIds,
            TotalCount = designs.Count
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var design = await _designService.GetDesignByIdAsync(id);
        if (design == null)
        {
            return NotFound();
        }

        await _designService.IncrementViewCountAsync(id);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_DesignModalPartial", design);
        }

        return View(design);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        bool isFavorited = await _designService.ToggleFavoriteAsync(userId, request.DesignId);
        return Json(new { success = true, isFavorited });
    }

    public class FavoriteRequest
    {
        public int DesignId { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using MarkGraphiko.Models.ViewModels;
using MarkGraphiko.Services;

namespace MarkGraphiko.Controllers;

public class HomeController : Controller
{
    private readonly IDesignService _designService;

    public HomeController(IDesignService designService)
    {
        _designService = designService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var featured = await _designService.GetFeaturedDesignsAsync();
        var recent = await _designService.GetRecentDesignsAsync(6);
        var categories = await _designService.GetCategoriesAsync();

        var viewModel = new HomeViewModel
        {
            FeaturedDesigns = featured,
            RecentWorks = recent,
            Categories = categories,
            TotalDesignsCount = featured.Count + recent.Count,
            HappyClientsCount = 84,
            YearsExperience = 6
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult About()
    {
        return View();
    }
}

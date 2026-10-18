using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarkGraphiko.Models.ViewModels;

public class UploadDesignViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public IFormFile? ImageFile { get; set; }

    [MaxLength(500)]
    public string? ExistingImagePath { get; set; }

    [MaxLength(100)]
    public string? Client { get; set; }

    [MaxLength(200)]
    public string? ToolsUsed { get; set; } = "Adobe Photoshop, Illustrator";

    [MaxLength(300)]
    public string? Tags { get; set; } = "Streetwear, Mockup, Minimal";

    public bool IsFeatured { get; set; } = true;

    public List<SelectListItem> CategoryOptions { get; set; } = new();
}

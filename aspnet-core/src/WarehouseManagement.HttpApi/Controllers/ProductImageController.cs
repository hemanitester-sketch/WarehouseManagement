using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace WarehouseManagement.Controllers;

[Route("api/app/product")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ProductImageController : WarehouseManagementController
{
    private readonly IWebHostEnvironment _environment;

    public ProductImageController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImageAsync()
    {
        var file = Request.HasFormContentType && Request.Form.Files.Count > 0
            ? (Request.Form.Files["file"] ?? Request.Form.Files[0])
            : null;

        if (file == null || file.Length == 0)
        {
            throw new UserFriendlyException("Please select an image file to upload.");
        }

        if (file.Length > 10 * 1024 * 1024)
        {
            throw new UserFriendlyException("Image file size cannot exceed 10 MB.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            throw new UserFriendlyException("Only image files (.jpg, .jpeg, .png, .webp, .gif, .svg) are allowed.");
        }

        var root = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadsDir = Path.Combine(root, "uploads", "products");
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/products/{fileName}";
        return Ok(new { url = relativeUrl });
    }
}

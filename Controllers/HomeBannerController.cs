using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HIVTraining_Vue.Data;
using HIVTraining_Vue.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HIVTraining_Vue.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeBannerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobContainerClient _bannerContainer;
        private bool _containerReady = false;

        private static readonly string[] AllowedImageExt = new[] { ".jpg", ".jpeg", ".png" };
        private const long MaxImageBytes = 500 * 1024; // 500 KB
        private const int RequiredWidth = 1600;
        private const int RequiredHeight = 900;

        public HomeBannerController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;

            var cs = config["Storage:ConnectionString"];
            var containerName = config["Storage:HomeBannerContainerName"] ?? "home-banners";

            var options = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2021_12_02);
            var serviceClient = new BlobServiceClient(cs, options);
            _bannerContainer = serviceClient.GetBlobContainerClient(containerName);
        }

        private async Task EnsureContainerAsync()
        {
            if (_containerReady) return;

            await _bannerContainer.CreateIfNotExistsAsync(PublicAccessType.None);
            _containerReady = true;
        }

        private static string GuessContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            return provider.TryGetContentType(fileName, out var ct) ? ct : "application/octet-stream";
        }

        private static async Task<string?> ValidateImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return "Banner image is required.";

            if (file.Length > MaxImageBytes)
                return "Image size must be under 500 KB.";

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExt.Contains(ext))
                return "Only JPG, JPEG, and PNG images are allowed.";

            try
            {
                using var stream = file.OpenReadStream();
                using var image = await Image.LoadAsync(stream);

                if (image.Width != RequiredWidth || image.Height != RequiredHeight)
                    return $"Image must be exactly {RequiredWidth} x {RequiredHeight} pixels.";
            }
            catch
            {
                return "Invalid image file.";
            }

            return null;
        }
        [HttpPut("toggle-active/{id:int}")]
        public async Task<IActionResult> ToggleActive(int id, [FromQuery] bool active)
        {
            var banner = await _context.HomeBanners.FirstOrDefaultAsync(x => x.HomeBannerSysId == id);

            if (banner == null)
                return NotFound(new { message = "Banner not found." });

            banner.Active = active;
            banner.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Banner {(active ? "activated" : "deactivated")} successfully.",
                banner.HomeBannerSysId,
                banner.Active
            });
        }

        private async Task<string> UploadImageAsync(int bannerId, IFormFile file)
        {
            await EnsureContainerAsync();

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var storedName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{ext}";
            var blobName = $"banners/{bannerId}/{storedName}";

            var blob = _bannerContainer.GetBlobClient(blobName);

            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                    ? GuessContentType(file.FileName)
                    : file.ContentType
            });

            return blobName;
        }

        [HttpGet("admin/list")]
        public async Task<IActionResult> GetAdminList()
        {
            var rows = await _context.HomeBanners
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.DateEntered)
                .Select(x => new
                {
                    x.HomeBannerSysId,
                    x.BannerName,
                    x.ActionType,
                    x.CourseSysId,
                    x.ModalTitle,
                    x.ModalBodyHtml,
                    x.ButtonText,
                    x.DisplayOrder,
                    x.Active,
                    x.StartDate,
                    x.EndDate,
                    x.DateEntered,
                    x.ImagePath,
                    ImageUrl = !string.IsNullOrEmpty(x.ImagePath)
                        ? $"/api/HomeBanner/image/{x.HomeBannerSysId}"
                        : null
                })
                .ToListAsync();

            return Ok(rows);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveBanners()
        {
            var now = DateTime.UtcNow;

            var rows = await _context.HomeBanners
                .Where(x => x.Active
                    && (!x.StartDate.HasValue || x.StartDate <= now)
                    && (!x.EndDate.HasValue || x.EndDate >= now))
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.DateEntered)
                .Select(x => new
                {
                    x.HomeBannerSysId,
                    x.BannerName,
                    x.ActionType,
                    x.CourseSysId,
                    x.ModalTitle,
                    x.ModalBodyHtml,
                    x.ButtonText,
                    x.DisplayOrder,
                    x.Active,
                    x.StartDate,
                    x.EndDate,
                    x.DateEntered,
                    x.DateModified,
                    ImageUrl = !string.IsNullOrEmpty(x.ImagePath)
                        ? $"/api/HomeBanner/image/{x.HomeBannerSysId}"
                        : null
                })
                .ToListAsync();

            return Ok(rows);
        }

        [HttpGet("image/{id:int}")]
        public async Task<IActionResult> GetImage(int id)
        {
            var banner = await _context.HomeBanners
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.HomeBannerSysId == id);

            if (banner == null)
                return NotFound(new { message = "Banner not found." });

            if (string.IsNullOrWhiteSpace(banner.ImagePath))
                return NotFound(new { message = "Banner image not found." });

            var blob = _bannerContainer.GetBlobClient(banner.ImagePath);

            if (!await blob.ExistsAsync())
                return NotFound(new { message = "Image missing in blob storage." });

            var fileName = banner.ImagePath.Split('/').Last();
            var contentType = GuessContentType(fileName);
            var dl = await blob.DownloadStreamingAsync();

            return File(dl.Value.Content, contentType, fileName);
        }

        [HttpPost("create")]
        [RequestSizeLimit(MaxImageBytes + 200000)]
        public async Task<IActionResult> Create([FromForm] HomeBannerUpsertRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.BannerName))
                return BadRequest(new { message = "Banner name is required." });

            var actionType = (req.ActionType ?? "").Trim();
            if (actionType != "Info" && actionType != "Course")
                return BadRequest(new { message = "ActionType must be either 'Info' or 'Course'." });

            if (actionType == "Course")
            {
                if (!req.CourseSysId.HasValue)
                    return BadRequest(new { message = "CourseSysId is required when ActionType is Course." });

                var courseExists = await _context.Courses.AnyAsync(x => x.CourseSysId == req.CourseSysId.Value);
                if (!courseExists)
                    return BadRequest(new { message = "Selected course was not found." });
            }

            if (actionType == "Info")
            {
                if (string.IsNullOrWhiteSpace(req.ModalTitle))
                    return BadRequest(new { message = "Modal title is required for Info banners." });

                if (string.IsNullOrWhiteSpace(req.ModalBodyHtml))
                    return BadRequest(new { message = "Modal content is required for Info banners." });
            }

            var imageError = await ValidateImageAsync(req.File);
            if (imageError != null)
                return BadRequest(new { message = imageError });

            var banner = new HomeBanner
            {
                BannerName = req.BannerName.Trim(),
                ActionType = actionType,
                CourseSysId = actionType == "Course" ? req.CourseSysId : null,
                ModalTitle = actionType == "Info" ? req.ModalTitle : null,
                ModalBodyHtml = actionType == "Info" ? req.ModalBodyHtml : null,
                ButtonText = req.ButtonText,
                DisplayOrder = req.DisplayOrder,
                Active = req.Active,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                DateEntered = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };

            _context.HomeBanners.Add(banner);
            await _context.SaveChangesAsync();

            if (req.File != null)
            {
                banner.ImagePath = await UploadImageAsync(banner.HomeBannerSysId, req.File);
                banner.DateModified = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Home banner created successfully.",
                bannerId = banner.HomeBannerSysId
            });
        }

        [HttpPut("update/{id:int}")]
        [RequestSizeLimit(MaxImageBytes + 200000)]
        public async Task<IActionResult> Update(int id, [FromForm] HomeBannerUpsertRequest req)
        {
            var banner = await _context.HomeBanners.FirstOrDefaultAsync(x => x.HomeBannerSysId == id);
            if (banner == null)
                return NotFound(new { message = "Banner not found." });

            if (string.IsNullOrWhiteSpace(req.BannerName))
                return BadRequest(new { message = "Banner name is required." });

            var actionType = (req.ActionType ?? "").Trim();
            if (actionType != "Info" && actionType != "Course")
                return BadRequest(new { message = "ActionType must be either 'Info' or 'Course'." });

            if (actionType == "Course")
            {
                if (!req.CourseSysId.HasValue)
                    return BadRequest(new { message = "CourseSysId is required when ActionType is Course." });

                var courseExists = await _context.Courses.AnyAsync(x => x.CourseSysId == req.CourseSysId.Value);
                if (!courseExists)
                    return BadRequest(new { message = "Selected course was not found." });
            }

            if (actionType == "Info")
            {
                if (string.IsNullOrWhiteSpace(req.ModalTitle))
                    return BadRequest(new { message = "Modal title is required for Info banners." });

                if (string.IsNullOrWhiteSpace(req.ModalBodyHtml))
                    return BadRequest(new { message = "Modal content is required for Info banners." });
            }

            if (req.File != null)
            {
                var imageError = await ValidateImageAsync(req.File);
                if (imageError != null)
                    return BadRequest(new { message = imageError });
            }

            banner.BannerName = req.BannerName.Trim();
            banner.ActionType = actionType;
            banner.CourseSysId = actionType == "Course" ? req.CourseSysId : null;
            banner.ModalTitle = actionType == "Info" ? req.ModalTitle : null;
            banner.ModalBodyHtml = actionType == "Info" ? req.ModalBodyHtml : null;
            banner.ButtonText = req.ButtonText;
            banner.DisplayOrder = req.DisplayOrder;
            banner.Active = req.Active;
            banner.StartDate = req.StartDate;
            banner.EndDate = req.EndDate;
            banner.DateModified = DateTime.UtcNow;

            if (req.File != null)
            {
                if (!string.IsNullOrWhiteSpace(banner.ImagePath))
                {
                    try
                    {
                        await _bannerContainer.GetBlobClient(banner.ImagePath).DeleteIfExistsAsync();
                    }
                    catch { }
                }

                banner.ImagePath = await UploadImageAsync(banner.HomeBannerSysId, req.File);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Home banner updated successfully." });
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _context.HomeBanners.FirstOrDefaultAsync(x => x.HomeBannerSysId == id);
            if (banner == null)
                return NotFound(new { message = "Banner not found." });

            if (!string.IsNullOrWhiteSpace(banner.ImagePath))
            {
                try
                {
                    await _bannerContainer.GetBlobClient(banner.ImagePath).DeleteIfExistsAsync();
                }
                catch { }
            }

            _context.HomeBanners.Remove(banner);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Home banner deleted successfully." });
        }
    }
}
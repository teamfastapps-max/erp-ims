using IMS.Models.Auth;
using IMS.Models.TenantUser;
using IMS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Web.Controllers
{
    [Authorize]
    public class TenantUserProfileController : Controller
    {
        private readonly IUserApiService _userApi;
        private readonly IUserSessionService _userSessionService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<TenantUserProfileController> _logger;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
        private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB

        public TenantUserProfileController(IUserApiService userApi, IUserSessionService userSessionService, IWebHostEnvironment env, ILogger<TenantUserProfileController> logger)
        {
            _userApi = userApi;
            _userSessionService = userSessionService;
            _env = env;
            _logger = logger;
        }

        // GET: /Profile
        public async Task<IActionResult> Index()
        {
            try
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized();
                }
                var profile = await _userApi.GetMyProfileAsync(accessToken);
                if (profile == null)
                {
                    TempData["ErrorMessage"] = "Could not load your profile. Please try again.";
                    return RedirectToAction("Index", "Home");
                }

                return View(MapToViewModel(profile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile for {Username}", User.Identity?.Name);
                TempData["ErrorMessage"] = "Could not load your profile. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /Profile/Update (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model?.Id))
                    return Json(new { success = false, message = "Missing profile Id." });

                var accessToken = await HttpContext.GetTokenAsync("access_token");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized();
                }
                var request = new UpdateMyProfileRequest
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    ProfilePic = model.ProfilePic,
                    Location = new LocationModel
                    {
                        Label = "Primary",
                        AddressLine1 = model.AddressLine1,
                        AddressLine2 = model.AddressLine2,
                        City = model.City,
                        State = model.State,
                        PostalCode = model.PostalCode,
                        Country = model.Country
                    }
                };

                var result = await _userApi.UpdateMyProfileAsync(model.Id, request, accessToken);
                if (!result.Success)
                    return Json(new { success = false, message = result.ErrorMessage ?? "Could not update your profile. Please try again." });
                //Will check it later
                //try
                //{
                //    await _userSessionService.StoreUserSessionAsync(User, accessToken);
                //}
                //catch (Exception sessionEx)
                //{
                //    _logger.LogWarning(sessionEx, "Profile updated but session cache refresh failed for {Username}", User.Identity?.Name);
                //}

                return Json(new { success = true, message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error updating profile for {Username}", User.Identity?.Name);
                return Json(new { success = false, message = "Something went wrong while updating your profile. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxFileSizeBytes + 1024)] 
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "Please choose a photo to upload." });

                if (file.Length > MaxFileSizeBytes)
                    return Json(new { success = false, message = "Photo must be 2MB or smaller." });

                var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                var contentType = file.ContentType?.ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || Array.IndexOf(AllowedExtensions, extension) < 0
                    || string.IsNullOrEmpty(contentType) || Array.IndexOf(AllowedContentTypes, contentType) < 0)
                {
                    return Json(new { success = false, message = "Only JPG, PNG, GIF or WEBP images are allowed." });
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "TenantUserProfile");
                Directory.CreateDirectory(uploadsFolder); 

                var fileName = $"{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativeUrl = $"/uploads/TenantUserProfile/{fileName}";
                return Json(new { success = true, message = "Photo uploaded.", url = relativeUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile photo for {Username}", User.Identity?.Name);
                return Json(new { success = false, message = "Could not upload the photo. Please try again." });
            }
        }

        private static ProfileViewModel MapToViewModel(UserProfileModel p) => new()
        {
            Id = p.Id,
            Email = p.Email,
            UserType = p.UserType,
            RoleName = p.CustomRoleName,
            TenantName = p.TenantDetails?.Name,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Phone = p.Phone,
            ProfilePic = p.ProfilePic,
            AddressLine1 = p.Location?.AddressLine1,
            AddressLine2 = p.Location?.AddressLine2,
            City = p.Location?.City,
            State = p.Location?.State,
            PostalCode = p.Location?.PostalCode,
            Country = p.Location?.Country
        };
    }
}

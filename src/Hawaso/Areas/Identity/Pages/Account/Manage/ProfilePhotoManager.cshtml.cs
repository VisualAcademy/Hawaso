#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace Azunt.Web.Areas.Identity.Pages.Account.Manage
{
    public class ProfilePhotoManagerModel : PageModel
    {
        private const long MaxProfilePhotoSize = 5 * 1024 * 1024;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfilePhotoManagerModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Display(Name = "Profile Photo")]
            public byte[]? ProfilePhoto { get; set; }
        }

        private Task LoadAsync(ApplicationUser user)
        {
            Input = new InputModel
            {
                ProfilePhoto = user.ProfilePicture
            };

            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnGetDownloadProfilePhotoAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("Unable to load current user.");
            }

            if (user.ProfilePicture == null || user.ProfilePicture.Length == 0)
            {
                return NotFound("No profile photo found.");
            }

            var imageInfo = DetectImageInfo(user.ProfilePicture);
            var safeUserName = CreateSafeFileName(user.UserName ?? "user");
            var fileName = $"{safeUserName}-profile-photo{imageInfo.Extension}";

            return File(user.ProfilePicture, imageInfo.ContentType, fileName);
        }

        public async Task<IActionResult> OnPostUploadProfilePhotoAsync(IFormFile? profilePhoto)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("Unable to load current user.");
            }

            var validationError = ValidateProfilePhoto(profilePhoto);

            if (!string.IsNullOrEmpty(validationError))
            {
                return BadRequest(validationError);
            }

            await SaveProfilePhotoAsync(user, profilePhoto!);

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return BadRequest("Unable to update profile photo.");
            }

            await _signInManager.RefreshSignInAsync(user);

            return new OkResult();
        }

        private static string? ValidateProfilePhoto(IFormFile? profilePhoto)
        {
            if (profilePhoto == null || profilePhoto.Length == 0)
            {
                return "Profile photo file is required.";
            }

            if (profilePhoto.Length > MaxProfilePhotoSize)
            {
                return "Profile photo must be 5MB or smaller.";
            }

            if (!string.IsNullOrWhiteSpace(profilePhoto.ContentType) &&
                !profilePhoto.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return "Only image files are allowed.";
            }

            return null;
        }

        private static async Task SaveProfilePhotoAsync(ApplicationUser user, IFormFile profilePhoto)
        {
            using (var dataStream = new MemoryStream())
            {
                await profilePhoto.CopyToAsync(dataStream);
                user.ProfilePicture = dataStream.ToArray();
            }
        }

        private static ImageFileInfo DetectImageInfo(byte[] bytes)
        {
            if (bytes.Length >= 12)
            {
                if (bytes[0] == 0x89 &&
                    bytes[1] == 0x50 &&
                    bytes[2] == 0x4E &&
                    bytes[3] == 0x47)
                {
                    return new ImageFileInfo("image/png", ".png");
                }

                if (bytes[0] == 0xFF &&
                    bytes[1] == 0xD8)
                {
                    return new ImageFileInfo("image/jpeg", ".jpg");
                }

                if (bytes[0] == 0x47 &&
                    bytes[1] == 0x49 &&
                    bytes[2] == 0x46)
                {
                    return new ImageFileInfo("image/gif", ".gif");
                }

                if (bytes[0] == 0x52 &&
                    bytes[1] == 0x49 &&
                    bytes[2] == 0x46 &&
                    bytes[3] == 0x46 &&
                    bytes[8] == 0x57 &&
                    bytes[9] == 0x45 &&
                    bytes[10] == 0x42 &&
                    bytes[11] == 0x50)
                {
                    return new ImageFileInfo("image/webp", ".webp");
                }

                if ((bytes[0] == 0x49 &&
                     bytes[1] == 0x49 &&
                     bytes[2] == 0x2A &&
                     bytes[3] == 0x00) ||
                    (bytes[0] == 0x4D &&
                     bytes[1] == 0x4D &&
                     bytes[2] == 0x00 &&
                     bytes[3] == 0x2A))
                {
                    return new ImageFileInfo("image/tiff", ".tif");
                }
            }

            return new ImageFileInfo("application/octet-stream", ".bin");
        }

        private static string CreateSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "user";
            }

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '-');
            }

            return value.Replace('@', '-').Replace('.', '-').Trim('-');
        }

        private sealed class ImageFileInfo
        {
            public ImageFileInfo(string contentType, string extension)
            {
                ContentType = contentType;
                Extension = extension;
            }

            public string ContentType { get; }

            public string Extension { get; }
        }
    }
}
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Hawaso.Areas.Identity.Pages.Account.Manage
{
    public class ProfilePictureUploadModel : PageModel
    {
        private const long MaxProfilePictureSize = 5 * 1024 * 1024;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfilePictureUploadModel(
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
            [Display(Name = "Profile Picture")]
            public byte[]? ProfilePicture { get; set; }
        }

        private Task LoadAsync(ApplicationUser user)
        {
            Input = new InputModel
            {
                ProfilePicture = user.ProfilePicture
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

        public async Task<IActionResult> OnPostUploadProfilePictureAsync(IFormFile? profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("Unable to load current user.");
            }

            var validationError = ValidateProfilePicture(profilePicture);

            if (!string.IsNullOrEmpty(validationError))
            {
                return BadRequest(validationError);
            }

            await SaveProfilePictureAsync(user, profilePicture!);

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return BadRequest("Unable to update profile picture.");
            }

            await _signInManager.RefreshSignInAsync(user);

            return new OkResult();
        }

        private static string? ValidateProfilePicture(IFormFile? profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                return "Profile picture file is required.";
            }

            if (profilePicture.Length > MaxProfilePictureSize)
            {
                return "Profile picture must be 5MB or smaller.";
            }

            if (!string.IsNullOrWhiteSpace(profilePicture.ContentType) &&
                !profilePicture.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return "Only image files are allowed.";
            }

            return null;
        }

        private static async Task SaveProfilePictureAsync(ApplicationUser user, IFormFile profilePicture)
        {
            using (var dataStream = new MemoryStream())
            {
                await profilePicture.CopyToAsync(dataStream);
                user.ProfilePicture = dataStream.ToArray();
            }
        }
    }
}
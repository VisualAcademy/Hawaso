using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Hawaso.Areas.Identity.Pages.Account
{
    public class GenerateTempPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public GenerateTempPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var password = GenerateRandomPassword(_userManager.Options.Password);
                    var resetPasswordResult = await _userManager.ResetPasswordAsync(user, token, password);

                    if (resetPasswordResult.Succeeded)
                    {
                        await _emailSender.SendEmailAsync(Input.Email, "Your temporary password", $"Your temporary password is {password}");
                        // redirect to confirmation page
                        return RedirectToPage("./GenerateTempPasswordConfirmation");
                    }
                }
            }
            return Page();
        }

        private string GenerateRandomPassword(PasswordOptions opts)
        {
            int length = opts.RequiredLength;

            // Create a string of characters that are allowed in the password
            string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            // If non-alphanumeric characters are required, add a few to the string
            if (opts.RequireNonAlphanumeric)
            {
                chars += "!@#$%^&*()";
            }

            // Generate the password
            var rng = new RNGCryptoServiceProvider();
            var buffer = new byte[length];
            rng.GetBytes(buffer);
            var password = new StringBuilder(length);
            foreach (byte b in buffer)
            {
                password.Append(chars[b % chars.Length]);
            }

            return password.ToString();
        }
    }
}

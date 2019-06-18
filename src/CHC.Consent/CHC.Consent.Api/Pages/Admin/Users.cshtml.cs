using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CHC.Consent.EFCore.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CHC.Consent.Api.Pages.Admin
{
    public class Users : PageModel
    {
        private readonly UserManager<ConsentUser> userManager;

        /// <inheritdoc />
        public Users(UserManager<ConsentUser> userManager)
        {
            this.userManager = userManager;
        }

        public void OnGet()
        {

        }

        [BindProperty(SupportsGet = false)]
        public DeactivateModel Deactivate { get; set; }
        
        [BindProperty(SupportsGet = false)]
        public CreateModel Create { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task<IActionResult> OnPostDeactivateAsync()
        {
            var user = await userManager.FindByIdAsync(Deactivate.UserId);
            if (user != null)
            {
                user.Deleted = user.Deleted ?? DateTime.Now;
            Message = $"User '{user.UserName}' deactivated";
                await userManager.UpdateAsync(user);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid) return Page();
            var result = await userManager.CreateAsync(
                new ConsentUser {Email = Create.EmailAddress, UserName = Create.UserName}, Create.Password);

            if (result.Succeeded)
            {
                Message = $"User '{Create.UserName}' created";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        public class DeactivateModel
        {
            [HiddenInput(DisplayValue = false)]
            public string UserId { get; set; }
        }

        public class CreateModel
        {
            [Required]
            public string UserName { get; set; }

            [EmailAddress, Required]
            public string EmailAddress { get; set; }

            [DataType(DataType.Password), Required, MinLength(6)]
            public string Password { get; set; }

            [Compare(nameof(Password))]
            public string ConfirmPassword { get; set; }

        }
    }

}
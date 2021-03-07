using Hawaso.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Hawaso.Pages.Administrations.Roles
{
    public partial class RoleCreate
    {        
        [Inject]
        public NavigationManager NavigationManagerRef { get; set; }

        [Inject]
        public RoleManager<ApplicationRole> RoleManager { get; set; }

        public ApplicationRoleViewModel ViewModel { get; set; } = new ApplicationRoleViewModel();

        public List<string> ErrorMessages { get; set; } = new List<string>();

        public async Task HandleCreation()
        {
            ApplicationRole identityRole = new ApplicationRole() 
            { 
                Name = ViewModel.RoleName,
            };

            IdentityResult identityResult = await RoleManager.CreateAsync(identityRole);
            if (identityResult.Succeeded)
            {
                NavigationManagerRef.NavigateTo("/Administrations/Roles");
            }
            else
            {
                foreach (var error in identityResult.Errors)
                {
                    ErrorMessages.Add(error.Description); 
                }
            }
        }
    }
}

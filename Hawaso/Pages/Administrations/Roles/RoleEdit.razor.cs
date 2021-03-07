using Hawaso.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hawaso.Pages.Administrations.Roles
{
    public partial class RoleEdit
    {
        [Parameter]
        public string Id { get; set; }

        [Inject]
        public NavigationManager NavigationManagerRef { get; set; }

        [Inject]
        public RoleManager<ApplicationRole> RoleManager { get; set; }

        public ApplicationRoleViewModel ViewModel { get; set; } = new ApplicationRoleViewModel();

        public List<string> ErrorMessages { get; set; } = new List<string>();

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        private ApplicationRole model = new ApplicationRole();

        protected override async Task OnInitializedAsync()
        {
            model = await RoleManager.FindByIdAsync(Id);
            ViewModel.Id = model.Id;
            ViewModel.RoleName = model.Name;
        }

        public async Task HandleUpdate()
        {
            var role = await RoleManager.FindByIdAsync(Id);
            if (role != null)
            {
                role.Name = ViewModel.RoleName;

                IdentityResult identityResult = await RoleManager.UpdateAsync(role);
                if (identityResult.Succeeded)
                {
                    NavigationManagerRef.NavigateTo("/Administrations/Roles/RoleDetails/" + Id);
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
}

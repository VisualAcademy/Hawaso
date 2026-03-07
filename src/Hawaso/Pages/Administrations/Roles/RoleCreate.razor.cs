using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.Administrations.Roles;

public partial class RoleCreate
{
    [Inject]
    public NavigationManager NavigationManagerRef { get; set; } = default!;

    [Inject]
    public RoleManager<ApplicationRole> RoleManager { get; set; } = default!;

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

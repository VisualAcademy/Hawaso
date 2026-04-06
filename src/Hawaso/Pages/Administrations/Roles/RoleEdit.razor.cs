using Hawaso.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace Hawaso.Pages.Administrations.Roles;

public partial class RoleEdit
{
    [Parameter]
    public string Id { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerRef { get; set; } = default!;

    [Inject]
    public RoleManager<ApplicationRole> RoleManager { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    public ApplicationRoleViewModel ViewModel { get; set; } = new();

    public List<string> ErrorMessages { get; set; } = new();

    private ApplicationRole model = new();

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            ErrorMessages.Add("Role ID is missing.");
            return;
        }

        model = await RoleManager.FindByIdAsync(Id) ?? new ApplicationRole();

        if (string.IsNullOrEmpty(model.Id))
        {
            ErrorMessages.Add("The requested role could not be found.");
            return;
        }

        ViewModel.Id = model.Id;
        ViewModel.RoleName = model.Name ?? string.Empty;
    }

    public async Task HandleUpdate()
    {
        ErrorMessages.Clear();

        if (string.IsNullOrWhiteSpace(Id))
        {
            ErrorMessages.Add("Role ID is missing.");
            return;
        }

        var role = await RoleManager.FindByIdAsync(Id);
        if (role == null)
        {
            ErrorMessages.Add("The requested role could not be found.");
            return;
        }

        role.Name = ViewModel.RoleName;

        IdentityResult identityResult = await RoleManager.UpdateAsync(role);
        if (identityResult.Succeeded)
        {
            NavigationManagerRef.NavigateTo($"/Administrations/Roles/RoleDetails/{Id}");
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
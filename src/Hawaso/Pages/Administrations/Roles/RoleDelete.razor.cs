using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Hawaso.Pages.Administrations.Roles;

public partial class RoleDelete
{
    [Parameter]
    public string Id { get; set; } = default!;

    [Inject]
    public RoleManager<ApplicationRole> RoleManager { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerRef { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    private ApplicationRole? model;

    public List<string> ErrorMessages { get; set; } = new();

    public bool ShowModal { get; set; } = false;
    public bool ShowBuiltIn { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        model = await RoleManager.FindByIdAsync(Id);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && model is null)
        {
            await JSRuntime.InvokeVoidAsync("alert", "잘못된 요청입니다.");
            NavigationManagerRef.NavigateTo("/Administrations/Roles");
        }
    }

    protected void HandleDelete() => ShowModal = true;

    protected void CloseModal() => ShowModal = false;

    private async Task DeleteProcess()
    {
        if (model is null)
        {
            return;
        }

        var normalizedName = model.NormalizedName;

        if (normalizedName is "ADMINISTRATORS" or "USERS" or "GUESTS" or "EVERYONE")
        {
            // 빌트인 역할은 필수 구성 역할이기 때문에 삭제할 수 없습니다.
            ShowBuiltIn = true;
            return;
        }

        IdentityResult identityResult = await RoleManager.DeleteAsync(model);

        if (identityResult.Succeeded)
        {
            NavigationManagerRef.NavigateTo("/Administrations/Roles");
        }
        else
        {
            ErrorMessages.Clear();

            foreach (var error in identityResult.Errors)
            {
                ErrorMessages.Add(error.Description);
            }
        }
    }

    protected void OpenBuiltIn() => ShowBuiltIn = true;

    protected void CloseBuiltIn()
    {
        ShowBuiltIn = false;
        ShowModal = false;
    }
}
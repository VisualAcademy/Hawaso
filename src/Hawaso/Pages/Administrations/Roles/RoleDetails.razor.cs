using Hawaso.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Hawaso.Pages.Administrations.Roles;

public partial class RoleDetails
{
    [Parameter]
    public string Id { get; set; } = string.Empty;

    [Inject]
    public RoleManager<ApplicationRole> RoleManager { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerRef { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    #region Properties

    private ApplicationRole? Model { get; set; }

    #endregion

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            Model = null;
            return;
        }

        Model = await RoleManager.FindByIdAsync(Id);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || Model is not null)
        {
            return;
        }

        await JSRuntime.InvokeVoidAsync(
            "alert",
            "잘못된 요청입니다.");

        NavigationManagerRef.NavigateTo(
            "/Administrations/Roles");
    }
}
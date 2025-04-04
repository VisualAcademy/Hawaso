using Hawaso.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Hawaso.Pages.Administrations.Roles;

public partial class RoleDetails
{
    [Parameter]
    public string Id { get; set; }

    [Inject]
    public RoleManager<ApplicationRole> RoleManager { get; set; }

    [Inject]
    public NavigationManager NavigationManagerRef { get; set; }

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    #region Properties
    private ApplicationRole Model = new ApplicationRole();  
    #endregion

    protected override async Task OnInitializedAsync()
    {
        Model = await RoleManager.FindByIdAsync(Id);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Model == null)
        {
            await JSRuntime.InvokeVoidAsync("alert", "잘못된 요청입니다.");
            NavigationManagerRef.NavigateTo("/Administrations/Roles");
        }
    }
}

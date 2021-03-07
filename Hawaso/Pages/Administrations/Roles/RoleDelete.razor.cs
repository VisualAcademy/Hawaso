using Hawaso.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hawaso.Pages.Administrations.Roles
{
    public partial class RoleDelete
    {
        [Parameter]
        public string Id { get; set; }

        [Inject]
        public RoleManager<ApplicationRole> RoleManager { get; set; }

        [Inject]
        public NavigationManager NavigationManagerRef { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        private ApplicationRole model = new ApplicationRole();

        public List<string> ErrorMessages { get; set; } = new List<string>();

        public bool ShowModal { get; set; } = false;
        public bool ShowBuiltIn { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            model = await RoleManager.FindByIdAsync(Id);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && model == null)
            {
                await JSRuntime.InvokeVoidAsync("alert", "잘못된 요청입니다.");
                NavigationManagerRef.NavigateTo("/Administrations/Roles");
            }
        }

        protected void HandleDelete()
        {
            ShowModal = true;
        }

        protected void CloseModal()
        {
            ShowModal = false;
        }

        private async Task DeleteProcess()
        {
            //bool isDelete = await JSRuntime.InvokeAsync<bool>("confirm", "정말로 삭제하시겠습니까?");
            //if (model != null && isDelete)
            if (model != null)
            {
                string r = model.NormalizedName;
                if (r == "ADMINISTRATORS" || r == "USERS" || r == "GUESTS" || r == "EVERYONE")
                {
                    // 빌트인 역할은 필수 구성 역할이기 때문에 삭제할 수 없습니다. 
                    ShowBuiltIn = true;
                }
                else
                {
                    IdentityResult identityResult = await RoleManager.DeleteAsync(model);
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

        protected void OpenBuiltIn()
        {
            ShowBuiltIn = true;
        }

        protected void CloseBuiltIn()
        {
            ShowBuiltIn = false;
            ShowModal = false;
        }
    }
}

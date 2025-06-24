using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azunt.Web.Components.Pages.Admin._01_Roles
{
    public partial class RoleList : ComponentBase
    {
        [Inject]
        public RoleManager<ApplicationRole> RoleManager { get; set; } = null!;

        protected List<ApplicationRoleDto> models = new();

        protected override async Task OnInitializedAsync()
        {
            var roles = RoleManager.Roles.OrderBy(m => m.Name).ToList();

            var dtos = roles.Select(role => new ApplicationRoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Order = role.Name switch
                {
                    "Administrators" => 1,
                    "Users" => 2,
                    _ => 99
                }
            }).ToList();

            models = dtos.OrderBy(d => d.Order).ThenBy(d => d.Name).ToList();
            await Task.CompletedTask;
        }

        protected bool IsSystemRole(string? roleName) =>
            roleName == "Administrators" || roleName == "Users";

        protected string EditLink(ApplicationRoleDto role) =>
            IsSystemRole(role.Name) ? "#" : $"/Administrations/Roles/RoleEdit/{role.Id}";

        protected string DeleteLink(ApplicationRoleDto role) =>
            IsSystemRole(role.Name) ? "#" : $"/Administrations/Roles/RoleDelete/{role.Id}";
    }

    public class ApplicationRoleDto : IdentityRole
    {
        public string? Description { get; set; }
        public int Order { get; set; } = 0;
    }
}

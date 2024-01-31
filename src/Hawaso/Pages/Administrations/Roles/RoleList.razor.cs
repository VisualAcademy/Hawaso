using Hawaso.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Administrations.Roles
{
    public partial class RoleList
    {
        [Inject]
        public RoleManager<ApplicationRole> RoleManager { get; set; }

        private List<ApplicationRole> models; 

        protected override async Task OnInitializedAsync()
        {
            models = RoleManager.Roles.OrderBy(m => m.Name).ToList();
            await Task.CompletedTask;
        }
    }
}

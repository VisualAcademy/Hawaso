using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Azunt.ResourceManagement;

namespace Azunt.Web.Components.Pages.Resources;

public partial class Details : ComponentBase
{
    [Parameter]
    public int Id { get; set; }

    protected Resource Model { get; set; } = new Resource();

    [Inject]
    public IResourceRepository RepositoryReference { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        Model = await RepositoryReference.GetByIdAsync(Id);
    }
}

using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using VisualAcademy.Models.Uploads;

namespace Hawaso.Pages.Uploads;

public partial class Details
{
    #region Fields
    protected string content = "";
    #endregion

    #region Parameters
    [Parameter]
    public int Id { get; set; }
    #endregion

    #region Properties
    public Upload Model { get; set; } = new Upload();
    #endregion

    #region Injectors
    [Inject]
    public IUploadRepository UploadRepositoryAsyncReference { get; set; }
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        Model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);
        content = Dul.HtmlUtility.EncodeWithTabAndSpace(Model.Content);
    }
    #endregion
}

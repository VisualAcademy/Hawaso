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

    #region Injectors
    [Inject]
    public IUploadRepository UploadRepositoryAsyncReference { get; set; }
    #endregion

    protected Upload Model = new Upload();

    protected override async Task OnInitializedAsync()
    {
        Model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);
        content = Dul.HtmlUtility.EncodeWithTabAndSpace(Model.Content);
    }
}

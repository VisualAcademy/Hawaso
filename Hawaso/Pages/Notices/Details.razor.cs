using Microsoft.AspNetCore.Components;
using NoticeApp.Models;
using System.Threading.Tasks;

namespace Hawaso.Pages.Notices;

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
    public INoticeRepository NoticeRepositoryReference { get; set; }
    #endregion

    #region Properties
    protected Notice model = new Notice();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        model = await NoticeRepositoryReference.GetByIdAsync(Id);
        //content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
        // HTML 태그 실행을 위한 인코딩없이 바로 전달
        content = model.Content;
    } 
    #endregion
}

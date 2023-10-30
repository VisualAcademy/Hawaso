using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Zero.Models;

namespace Hawaso.Pages.BriefingLogs;

public partial class Delete
{
    #region Parameters
    [Parameter]
    public int Id { get; set; }
    #endregion

    #region Injectors
    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; }

    [Inject]
    public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; }
    #endregion

    protected BriefingLog model = new();

    #region Fields
    protected string content = "";
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);
        content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
    }
    #endregion

    protected async void DeleteClick()
    {
        bool isDelete = await JSRuntime.InvokeAsync<bool>("confirm", $"{Id}번 글을 정말로 삭제하시겠습니까?");

        if (isDelete)
        {
            if (!string.IsNullOrEmpty(model?.FileName))
            {
                // 첨부 파일 삭제 
                await FileStorageManager.DeleteAsync(model.FileName, "BriefingLogs");
            }

            await UploadRepositoryAsyncReference.DeleteAsync(Id); // 삭제
            NavigationManagerReference.NavigateTo("/BriefingLogs"); // 리스트 페이지로 이동
        }
        else
        {
            await JSRuntime.InvokeAsync<object>("alert", "취소되었습니다.");
        }
    }

    [Inject]
    public IBriefingLogFileStorageManager FileStorageManager { get; set; }
}

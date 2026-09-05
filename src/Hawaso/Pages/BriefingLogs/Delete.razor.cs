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
    public IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; } = default!;

    [Inject]
    public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public IBriefingLogFileStorageManager FileStorageManager { get; set; } = default!;

    #endregion

    #region Fields

    protected string content = string.Empty;

    protected BriefingLog model = new();

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);
        content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
    }

    #endregion

    #region Event Handlers

    protected async Task DeleteClick()
    {
        bool isDelete = await JSRuntime.InvokeAsync<bool>(
            "confirm",
            $"{Id}번 글을 정말로 삭제하시겠습니까?");

        if (isDelete)
        {
            if (!string.IsNullOrEmpty(model.FileName))
            {
                // 첨부 파일 삭제
                await FileStorageManager.DeleteAsync(
                    model.FileName,
                    "BriefingLogs");
            }

            // 데이터 삭제
            await UploadRepositoryAsyncReference.DeleteAsync(Id);

            // 리스트 페이지로 이동
            NavigationManagerReference.NavigateTo("/BriefingLogs");
        }
        else
        {
            await JSRuntime.InvokeVoidAsync(
                "alert",
                "취소되었습니다.");
        }
    }

    #endregion
}
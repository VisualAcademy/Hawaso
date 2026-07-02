using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using Zero.Models;

namespace Hawaso.Pages.BriefingLogs.Components;

public partial class EditorForm
{
    /// <summary>
    /// 모달 다이얼로그를 표시할건지 여부
    /// </summary>
    public bool IsShow { get; set; }

    private string parentId = string.Empty;

    [Parameter]
    public string ParentKey { get; set; } = string.Empty;

    protected int[] parentIds = { 1, 2, 3 };

    /// <summary>
    /// 폼 보이기
    /// </summary>
    public void Show() => IsShow = true;

    /// <summary>
    /// 폼 닫기
    /// </summary>
    public void Hide() => IsShow = false;

    /// <summary>
    /// 폼의 제목 영역
    /// </summary>
    [Parameter]
    public RenderFragment? EditorFormTitle { get; set; }

    /// <summary>
    /// 넘어온 모델 개체
    /// </summary>
    [Parameter]
    public BriefingLog Model { get; set; } = new();

    /// <summary>
    /// 부모 컴포넌트에게 생성(Create)이 완료되었다고 보고하는 목적으로 부모 컴포넌트에게 알림
    /// </summary>
    [Parameter]
    public Action? CreateCallback { get; set; }

    /// <summary>
    /// 부모 컴포넌트에게 수정(Edit)이 완료되었다고 보고하는 목적으로 부모 컴포넌트에게 알림
    /// </summary>
    [Parameter]
    public EventCallback<bool> EditCallback { get; set; }

    /// <summary>
    /// 리포지토리 클래스에 대한 참조
    /// </summary>
    [Inject]
    public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public IBriefingLogFileStorageManager FileStorageManager { get; set; } = default!;

    private IFileListEntry[]? selectedFiles;

    protected override void OnParametersSet()
    {
        parentId = Model.ParentId?.ToString() ?? string.Empty;

        if (parentId == "0")
        {
            parentId = string.Empty;
        }
    }

    protected async Task CreateOrEditClick()
    {
        #region 파일 업로드 관련 추가 코드 영역

        if (selectedFiles is { Length: > 0 })
        {
            var file = selectedFiles.FirstOrDefault();

            if (file is not null)
            {
                var fileName = file.Name ?? string.Empty;
                var fileSize = Convert.ToInt32(file.Size);

                //[A] byte[] 형태
                //var ms = new MemoryStream();
                //await file.Data.CopyToAsync(ms);
                //await FileStorageManager.UploadAsync(ms.ToArray(), file.Name, "", true);

                //[B] Stream 형태
                //string folderPath = Path.Combine(WebHostEnvironment.WebRootPath, "files");
                await FileStorageManager.UploadAsync(file.Data, fileName, string.Empty, true);

                Model.FileName = fileName;
                Model.FileSize = fileSize;
            }
        }

        #endregion

        if (int.TryParse(parentId, out var parsedParentId))
        {
            Model.ParentId = parsedParentId;
        }
        else
        {
            Model.ParentId = null;
        }

        Model.ParentKey = ParentKey;

        if (Model.Id == 0)
        {
            // Create
            await UploadRepositoryAsyncReference.AddAsync(Model);
            CreateCallback?.Invoke();
        }
        else
        {
            // Edit
            await UploadRepositoryAsyncReference.EditAsync(Model);
            await EditCallback.InvokeAsync(true);
        }

        //IsShow = false; // this.Hide()
    }

    protected void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files;
    }
}
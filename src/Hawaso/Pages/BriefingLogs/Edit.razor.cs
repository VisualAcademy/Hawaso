using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using Zero.Models;

namespace Hawaso.Pages.BriefingLogs;

public partial class Edit
{
    #region Fields

    protected int[] parentIds = { 1, 2, 3 };

    protected string content = string.Empty;

    protected BriefingLog Model = new();

    #endregion

    #region Properties

    public string ParentId { get; set; } = string.Empty;

    #endregion

    #region Parameters

    [Parameter]
    public int Id { get; set; }

    #endregion

    #region Injectors

    [Inject]
    public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; } = default!;

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        Model = await UploadRepositoryAsyncReference.GetByIdAsync(Id)
            ?? throw new InvalidOperationException(
                $"BriefingLog with Id {Id} was not found.");

        content = Dul.HtmlUtility.EncodeWithTabAndSpace(
            Model.Content ?? string.Empty);

        ParentId = Model.ParentId?.ToString() ?? string.Empty;
    }

    #endregion

    #region Event Handlers

    protected async Task FormSubmit()
    {
        int.TryParse(ParentId, out int parentId);
        Model.ParentId = parentId;

        #region 파일 업로드 관련 추가 코드 영역

        if (selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();

            if (file != null)
            {
                var fileSize = Convert.ToInt32(file.Size);

                // 기존 첨부 파일 삭제
                if (!string.IsNullOrWhiteSpace(Model.FileName))
                {
                    await FileStorageManager.DeleteAsync(
                        Model.FileName,
                        "BriefingLogs");
                }

                // 새 파일 업로드
                var fileName = await FileStorageManager.UploadAsync(
                    file.Data,
                    file.Name,
                    "",
                    true);

                Model.FileName = fileName;
                Model.FileSize = fileSize;
            }
        }

        #endregion

        await UploadRepositoryAsyncReference.EditAsync(Model);

        NavigationManagerReference.NavigateTo("/BriefingLogs");
    }

    #endregion

    #region Features - FileUpload

    [Inject]
    public IBriefingLogFileStorageManager FileStorageManager { get; set; } = default!;

    private IFileListEntry[] selectedFiles = Array.Empty<IFileListEntry>();

    protected void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files;
    }

    #endregion
}
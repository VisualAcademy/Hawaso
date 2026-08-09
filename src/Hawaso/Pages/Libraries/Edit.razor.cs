using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Libraries;

namespace Hawaso.Pages.Libraries;

public partial class Edit
{
    #region Parameters

    [Parameter]
    public int Id { get; set; }

    #endregion

    #region Injectors

    [Inject]
    public ILibraryRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; } = default!;

    [Inject]
    public ILibraryFileStorageManager FileStorageManager { get; set; } = default!;

    #endregion

    #region Fields

    protected LibraryModel model = new();

    protected int[] parentIds = { 1, 2, 3 };

    protected string content = string.Empty;

    private IFileListEntry[] selectedFiles = Array.Empty<IFileListEntry>();

    #endregion

    #region Properties

    public string ParentId { get; set; } = string.Empty;

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        model = await UploadRepositoryAsyncReference.GetByIdAsync(Id)
            ?? throw new InvalidOperationException(
                $"Library with Id {Id} was not found.");

        content = Dul.HtmlUtility.EncodeWithTabAndSpace(
            model.Content ?? string.Empty);

        ParentId = model.ParentId?.ToString() ?? string.Empty;
    }

    #endregion

    #region Event Handlers

    protected async Task FormSubmit()
    {
        int.TryParse(ParentId, out int parentId);
        model.ParentId = parentId;

        #region 파일 업로드 관련 추가 코드 영역

        if (selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();

            if (file != null)
            {
                var fileSize = Convert.ToInt32(file.Size);

                // 기존 첨부 파일 삭제
                if (!string.IsNullOrWhiteSpace(model.FileName))
                {
                    await FileStorageManager.DeleteAsync(
                        model.FileName,
                        "Libraries");
                }

                // 새 파일 업로드
                var fileName = await FileStorageManager.UploadAsync(
                    file.Data,
                    file.Name,
                    "",
                    true);

                model.FileName = fileName;
                model.FileSize = fileSize;
            }
        }

        #endregion

        await UploadRepositoryAsyncReference.EditAsync(model);

        NavigationManagerReference.NavigateTo("/Libraries");
    }

    #endregion

    #region Features - FileUpload

    protected void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files;
    }

    #endregion
}
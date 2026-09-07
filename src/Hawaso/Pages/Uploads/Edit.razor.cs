using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Uploads;

public partial class Edit
{
    #region Parameters

    [Parameter]
    public int Id { get; set; }

    #endregion

    #region Injectors

    [Inject]
    public IUploadRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; } = default!;

    [Inject]
    public IFileStorageManager FileStorageManager { get; set; } = default!;

    #endregion

    #region Fields

    protected Upload model = new();

    protected int[] parentIds = { 1, 2, 3 };

    protected string content = string.Empty;

    /// <summary>
    /// 첨부 파일 리스트 보관
    /// </summary>
    private IFileListEntry[] selectedFiles = Array.Empty<IFileListEntry>();

    #endregion

    #region Properties

    public string ParentId { get; set; } = string.Empty;

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);

        content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);

        ParentId = model.ParentId is > 0
            ? model.ParentId.Value.ToString()
            : string.Empty;
    }

    #endregion

    #region Event Handlers

    protected async Task FormSubmit()
    {
        if (!int.TryParse(ParentId, out int parentId))
        {
            parentId = 0;
        }

        model.ParentId = parentId;

        #region 파일 업로드 관련 추가 코드 영역

        if (selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();

            if (file != null)
            {
                int fileSize = Convert.ToInt32(file.Size);

                // 기존 첨부 파일 삭제
                if (!string.IsNullOrWhiteSpace(model.FileName))
                {
                    await FileStorageManager.DeleteAsync(
                        model.FileName,
                        string.Empty);
                }

                // 새 첨부 파일 업로드
                string fileName = await FileStorageManager.UploadAsync(
                    file.Data,
                    file.Name,
                    string.Empty,
                    true);

                model.FileName = fileName;
                model.FileSize = fileSize;
            }
        }

        #endregion

        await UploadRepositoryAsyncReference.EditAsync(model);

        NavigationManagerReference.NavigateTo("/Uploads");
    }

    /// <summary>
    /// 파일 선택 이벤트 처리기
    /// </summary>
    protected void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files ?? Array.Empty<IFileListEntry>();
    }

    #endregion
}
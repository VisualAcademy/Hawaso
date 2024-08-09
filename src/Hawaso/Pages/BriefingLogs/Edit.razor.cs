using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using Zero.Models;

namespace Hawaso.Pages.BriefingLogs;

public partial class Edit
{
    #region Fields
    protected int[] parentIds = { 1, 2, 3 };

    protected string content = "";

    protected BriefingLog Model = new();
    #endregion

    #region Properties
    public string ParentId { get; set; }
    #endregion

    #region Parameters
    [Parameter]
    public int Id { get; set; }
    #endregion

    #region Injectors
    [Inject]
    public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; }

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; }
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        Model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);
        content = Dul.HtmlUtility.EncodeWithTabAndSpace(Model.Content);
        ParentId = Model.ParentId.ToString();
    }
    #endregion
    
    #region Event Handlers
    protected async void FormSubmit()
    {
        int.TryParse(ParentId, out int parentId);
        Model.ParentId = parentId;

        #region 파일 업로드 관련 추가 코드 영역
        if (selectedFiles != null && selectedFiles.Length > 0)
        {
            // 파일 업로드
            var file = selectedFiles.FirstOrDefault();
            var fileName = "";
            int fileSize = 0;
            if (file != null)
            {
                fileName = file.Name;
                fileSize = Convert.ToInt32(file.Size);

                // 첨부 파일 삭제 
                await FileStorageManager.DeleteAsync(Model.FileName, "BriefingLogs");

                // 다시 업로드
                fileName = await FileStorageManager.UploadAsync(file.Data, file.Name, "", true);

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
    public IBriefingLogFileStorageManager FileStorageManager { get; set; }
    private IFileListEntry[] selectedFiles;
    protected void HandleSelection(IFileListEntry[] files) => this.selectedFiles = files;
    #endregion
}

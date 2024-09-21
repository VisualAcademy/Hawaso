using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Uploads;

public partial class Create
{
    #region Injectors
    [Inject]
    public IUploadRepository UploadRepositoryAsyncReference { get; set; }

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; }
    #endregion

    #region Properties
    public Upload Model { get; set; }

    public string ParentId { get; set; }
    #endregion
    
    protected int[] parentIds = { 1, 2, 3 };

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

                fileName = await FileStorageManager.UploadAsync(file.Data, file.Name, "", true);

                Model.FileName = fileName;
                Model.FileSize = fileSize;
            } 
        }
        #endregion

        await UploadRepositoryAsyncReference.AddAsync(Model);
        NavigationManagerReference.NavigateTo("/Uploads");
    }

    [Inject]
    public IFileStorageManager FileStorageManager { get; set; }
    private IFileListEntry[] selectedFiles;
    protected void HandleSelection(IFileListEntry[] files) => this.selectedFiles = files;
}

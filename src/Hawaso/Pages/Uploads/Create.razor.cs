using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading.Tasks;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Uploads;

public partial class Create
{
    #region Injectors

    [Inject]
    public IUploadRepository UploadRepositoryAsyncReference { get; set; }
        = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; }
        = default!;

    [Inject]
    public IFileStorageManager FileStorageManager { get; set; }
        = default!;

    #endregion

    #region Properties

    public Upload Model { get; set; } = new();

    public string ParentId { get; set; } = string.Empty;

    #endregion

    protected int[] parentIds = [1, 2, 3];

    private IFileListEntry[] selectedFiles =
        Array.Empty<IFileListEntry>();

    protected async Task FormSubmit()
    {
        int.TryParse(ParentId, out var parentId);

        Model.ParentId = parentId;

        #region 파일 업로드 관련 추가 코드 영역

        var file = selectedFiles.FirstOrDefault();

        if (file is not null)
        {
            var uploadedFileName = await FileStorageManager.UploadAsync(
                file.Data,
                file.Name,
                string.Empty,
                overwrite: true);

            Model.FileName = uploadedFileName;
            Model.FileSize = Convert.ToInt32(file.Size);
        }

        #endregion

        await UploadRepositoryAsyncReference.AddAsync(Model);

        NavigationManagerReference.NavigateTo("/Uploads");
    }

    protected void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files ?? Array.Empty<IFileListEntry>();
    }
}
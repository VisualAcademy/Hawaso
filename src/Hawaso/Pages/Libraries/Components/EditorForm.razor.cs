using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Libraries;

namespace Hawaso.Pages.Libraries.Components;

public partial class EditorForm
{
    /// <summary>
    /// 모달 다이얼로그를 표시할건지 여부
    /// </summary>
    public bool IsShow { get; set; } = false;

    private string parentId = string.Empty;

    protected int[] parentIds = { 1, 2, 3 };

    private IFileListEntry[] selectedFiles = Array.Empty<IFileListEntry>();

    /// <summary>
    /// 폼의 제목 영역
    /// </summary>
    [Parameter]
    public RenderFragment? EditorFormTitle { get; set; }

    /// <summary>
    /// 넘어온 모델 개체
    /// </summary>
    [Parameter]
    public LibraryModel Model { get; set; } = new();

    [Parameter]
    public string ParentKey { get; set; } = string.Empty;

    /// <summary>
    /// 부모 컴포넌트에게 생성(Create)이 완료되었다고 보고한다.
    /// </summary>
    [Parameter]
    public EventCallback CreateCallback { get; set; }

    /// <summary>
    /// 부모 컴포넌트에게 수정(Edit)이 완료되었다고 보고한다.
    /// </summary>
    [Parameter]
    public EventCallback<bool> EditCallback { get; set; }

    /// <summary>
    /// 리포지토리 클래스에 대한 참조
    /// </summary>
    [Inject]
    public ILibraryRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public ILibraryFileStorageManager FileStorageManager { get; set; } = default!;

    /// <summary>
    /// 폼 보이기
    /// </summary>
    public void Show()
    {
        IsShow = true;
    }

    /// <summary>
    /// 폼 닫기
    /// </summary>
    public void Hide()
    {
        IsShow = false;
    }

    protected override void OnParametersSet()
    {
        Model ??= new LibraryModel();

        if (!string.IsNullOrWhiteSpace(ParentKey) && string.IsNullOrWhiteSpace(Model.ParentKey))
        {
            Model.ParentKey = ParentKey;
        }

        parentId = Model.ParentId == 0 ? string.Empty : Model.ParentId.ToString();
    }

    protected async Task CreateOrEditClick()
    {
        if (selectedFiles.Length > 0)
        {
            IFileListEntry? file = selectedFiles.FirstOrDefault();

            if (file != null)
            {
                string fileName = file.Name;
                int fileSize = Convert.ToInt32(file.Size);

                await FileStorageManager.UploadAsync(file.Data, file.Name, string.Empty, true);

                Model.FileName = fileName;
                Model.FileSize = fileSize;
            }
        }

        if (!int.TryParse(parentId, out int newParentId))
        {
            newParentId = 0;
        }

        Model.ParentId = newParentId;

        if (!string.IsNullOrWhiteSpace(ParentKey))
        {
            Model.ParentKey = ParentKey;
        }

        if (Model.Id == 0)
        {
            await UploadRepositoryAsyncReference.AddAsync(Model);
            await CreateCallback.InvokeAsync();
        }
        else
        {
            await UploadRepositoryAsyncReference.EditAsync(Model);
            await EditCallback.InvokeAsync(true);
        }

        selectedFiles = Array.Empty<IFileListEntry>();
    }

    protected void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files ?? Array.Empty<IFileListEntry>();
    }
}
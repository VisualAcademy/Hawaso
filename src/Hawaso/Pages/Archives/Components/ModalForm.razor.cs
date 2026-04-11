using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Archives;

namespace Hawaso.Pages.Archives.Components;

public partial class ModalForm
{
    #region Fields
    private string parentId = "";

    /// <summary>
    /// 첨부 파일 리스트 보관
    /// </summary>
    private IFileListEntry[]? selectedFiles;
    #endregion

    #region Properties
    /// <summary>
    /// (글쓰기/글수정)모달 다이얼로그를 표시할건지 여부
    /// </summary>
    public bool IsShow { get; set; } = false;
    #endregion

    #region Public Methods
    /// <summary>
    /// 폼 보이기
    /// </summary>
    public void Show() => IsShow = true;

    /// <summary>
    /// 폼 닫기
    /// </summary>
    public void Hide() => IsShow = false;
    #endregion

    #region Parameters
    /// <summary>
    /// 폼의 제목 영역
    /// </summary>
    [Parameter]
    public RenderFragment? EditorFormTitle { get; set; }

    /// <summary>
    /// 넘어온 모델 개체
    /// </summary>
    [Parameter]
    public Archive ModelSender { get; set; } = new();

    public Archive ModelEdit { get; set; } = new();

    public string[] Encodings { get; set; } = { "Plain-Text", "Text/HTML", "Mixed-Text" };

    #region Lifecycle Methods
    protected override void OnParametersSet()
    {
        ModelEdit = new Archive
        {
            Id = ModelSender.Id,
            Name = ModelSender.Name,
            Title = ModelSender.Title,
            Content = ModelSender.Content,
            Password = ModelSender.Password,
            Encoding = string.IsNullOrWhiteSpace(ModelSender.Encoding)
                ? "Plain-Text"
                : ModelSender.Encoding
        };

        parentId = ModelSender.ParentId.ToString();
        if (parentId == "0")
        {
            parentId = "";
        }
    }
    #endregion

    /// <summary>
    /// 부모 컴포넌트에게 생성(Create)이 완료되었다고 보고하는 목적으로 부모 컴포넌트에게 알림
    /// 학습 목적으로 Action 대리자 사용
    /// </summary>
    [Parameter]
    public Action? CreateCallback { get; set; }

    /// <summary>
    /// 부모 컴포넌트에게 수정(Edit)이 완료되었다고 보고하는 목적으로 부모 컴포넌트에게 알림
    /// 학습 목적으로 EventCallback 구조체 사용
    /// </summary>
    [Parameter]
    public EventCallback<bool> EditCallback { get; set; }

    [Parameter]
    public string ParentKey { get; set; } = "";
    #endregion

    #region Injectors
    /// <summary>
    /// 리포지토리 클래스에 대한 참조
    /// </summary>
    [Inject]
    public IArchiveRepository RepositoryReference { get; set; } = default!;

    [Inject]
    public IArchiveFileStorageManager FileStorageManagerReference { get; set; } = default!;
    #endregion

    #region Event Handlers
    protected async void CreateOrEditClick()
    {
        ModelSender.Name = ModelEdit.Name;
        ModelSender.Title = ModelEdit.Title;
        ModelSender.Content = ModelEdit.Content;
        ModelSender.Password = ModelEdit.Password;
        ModelSender.Encoding = ModelEdit.Encoding;

        if (selectedFiles is { Length: > 0 })
        {
            var file = selectedFiles.FirstOrDefault();
            if (file != null)
            {
                string fileName = file.Name;
                int fileSize = Convert.ToInt32(file.Size);

                await FileStorageManagerReference.UploadAsync(file.Data, file.Name, "Archives", true);

                ModelSender.FileName = fileName;
                ModelSender.FileSize = fileSize;
            }
        }

        if (!int.TryParse(parentId, out int newParentId))
        {
            newParentId = 0;
        }

        ModelSender.ParentId = newParentId;
        ModelSender.ParentKey = ParentKey;

        if (ModelSender.Id == 0)
        {
            await RepositoryReference.AddAsync(ModelSender);
            CreateCallback?.Invoke();
        }
        else
        {
            await RepositoryReference.UpdateAsync(ModelSender);
            await EditCallback.InvokeAsync(true);
        }
    }

    protected void HandleSelection(IFileListEntry[] files) => selectedFiles = files;
    #endregion
}
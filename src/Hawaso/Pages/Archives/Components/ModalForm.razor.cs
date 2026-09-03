using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Archives;

namespace Hawaso.Pages.Archives.Components;

public partial class ModalForm
{
    #region Fields

    private string parentId = string.Empty;

    /// <summary>
    /// 첨부 파일 리스트 보관
    /// </summary>
    private IFileListEntry[] selectedFiles = Array.Empty<IFileListEntry>();

    #endregion

    #region Properties

    /// <summary>
    /// (글쓰기/글수정)모달 다이얼로그를 표시할건지 여부
    /// </summary>
    public bool IsShow { get; set; }

    public Archive ModelEdit { get; set; } = new();

    public string[] Encodings { get; set; } =
    {
        "Plain-Text",
        "Text/HTML",
        "Mixed-Text"
    };

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

    /// <summary>
    /// 부모 컴포넌트에게 생성(Create)이 완료되었다고 보고하는 목적으로
    /// 부모 컴포넌트에게 알림
    /// 학습 목적으로 Action 대리자 사용
    /// </summary>
    [Parameter]
    public Action? CreateCallback { get; set; }

    /// <summary>
    /// 부모 컴포넌트에게 수정(Edit)이 완료되었다고 보고하는 목적으로
    /// 부모 컴포넌트에게 알림
    /// 학습 목적으로 EventCallback 구조체 사용
    /// </summary>
    [Parameter]
    public EventCallback<bool> EditCallback { get; set; }

    [Parameter]
    public string ParentKey { get; set; } = string.Empty;

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

    #region Lifecycle Methods

    protected override void OnParametersSet()
    {
        ModelEdit = new Archive
        {
            Id = ModelSender.Id,
            Name = ModelSender.Name ?? string.Empty,
            Title = ModelSender.Title ?? string.Empty,
            Content = ModelSender.Content ?? string.Empty,
            Password = ModelSender.Password ?? string.Empty,
            Encoding = string.IsNullOrWhiteSpace(ModelSender.Encoding)
                ? "Plain-Text"
                : ModelSender.Encoding
        };

        parentId = ModelSender.ParentId is > 0 ? ModelSender.ParentId.Value.ToString() : string.Empty;

        if (parentId == "0")
        {
            parentId = string.Empty;
        }

        // 파라미터가 다시 설정될 때 이전 파일 선택 상태 초기화
        selectedFiles = Array.Empty<IFileListEntry>();
    }

    #endregion

    #region Event Handlers

    protected async Task CreateOrEditClick()
    {
        ModelSender.Name = ModelEdit.Name ?? string.Empty;
        ModelSender.Title = ModelEdit.Title ?? string.Empty;
        ModelSender.Content = ModelEdit.Content ?? string.Empty;
        ModelSender.Password = ModelEdit.Password ?? string.Empty;
        ModelSender.Encoding = string.IsNullOrWhiteSpace(ModelEdit.Encoding)
            ? "Plain-Text"
            : ModelEdit.Encoding;

        if (selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();

            if (file != null)
            {
                string fileName = file.Name;
                int fileSize = Convert.ToInt32(file.Size);

                fileName = await FileStorageManagerReference.UploadAsync(
                    file.Data,
                    file.Name,
                    "Archives",
                    true);

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

    /// <summary>
    /// 파일 선택 이벤트 처리기
    /// </summary>
    protected void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files ?? Array.Empty<IFileListEntry>();
    }

    #endregion
}
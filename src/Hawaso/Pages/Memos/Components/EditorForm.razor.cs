using BlazorInputFile;
using Microsoft.AspNetCore.Components;

namespace VisualAcademy.Pages.Memos.Components;

public partial class EditorForm
{
    #region Fields

    private string parentId = "";

    protected int[] parentIds = { 1, 2, 3 };

    /// <summary>
    /// 첨부 파일 리스트 보관
    /// </summary>
    private IFileListEntry[] selectedFiles = Array.Empty<IFileListEntry>();

    #endregion

    #region Properties

    /// <summary>
    /// 모달 다이얼로그를 표시할건지 여부
    /// </summary>
    public bool IsShow { get; set; }

    /// <summary>
    /// 전체 넘어온 개체 중에서 폼에서 변경되는 내용만 따로 관리:
    /// ModelEdit => MemoEdit, MemoViewModel, ...
    /// </summary>
    public Memo ModelEdit { get; set; } = new();

    #endregion

    #region Parameters

    /// <summary>
    /// 폼의 제목 영역
    /// </summary>
    [Parameter]
    public RenderFragment? EditorFormTitle { get; set; }

    /// <summary>
    /// 넘어온 모델 개체
    /// 부모 컴포넌트에서 반드시 전달해야 하는 필수 파라미터입니다.
    /// </summary>
    [Parameter]
    public Memo Model { get; set; } = default!;

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
    public IMemoRepository RepositoryReference { get; set; } = default!;

    [Inject]
    public IMemoFileStorageManager FileStorageManagerReference { get; set; } = default!;

    #endregion

    #region Public Methods

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

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// 넘어온 Model 값을 수정 전용 ModelEdit에 담기
    /// </summary>
    protected override void OnParametersSet()
    {
        ModelEdit = new Memo
        {
            Id = Model.Id,
            Name = Model.Name,
            Title = Model.Title,
            Content = Model.Content,
            Password = Model.Password,
            ParentId = Model.ParentId,
            ParentKey = Model.ParentKey,
            FileName = Model.FileName,
            FileSize = Model.FileSize
        };

        parentId = Model.ParentId.HasValue && Model.ParentId.Value > 0
            ? Model.ParentId.Value.ToString()
            : "";
    }

    #endregion

    #region Event Handlers

    protected async Task CreateOrEditClick()
    {
        Model.Name = ModelEdit.Name;
        Model.Title = ModelEdit.Title;
        Model.Content = ModelEdit.Content;
        Model.Password = ModelEdit.Password;

        #region 파일 업로드 관련 추가 코드 영역

        if (selectedFiles.Length > 0)
        {
            IFileListEntry? file = selectedFiles.FirstOrDefault();

            if (file is not null)
            {
                string fileName = file.Name;
                int fileSize = Convert.ToInt32(file.Size);

                await FileStorageManagerReference.UploadAsync(file.Data, fileName, "Memos", true);

                Model.FileName = fileName;
                Model.FileSize = fileSize;
            }
        }

        #endregion

        if (long.TryParse(parentId, out long newParentId))
        {
            Model.ParentId = newParentId;
        }
        else
        {
            Model.ParentId = null;
        }

        Model.ParentKey = ParentKey;

        if (Model.Id == 0)
        {
            await RepositoryReference.AddAsync(Model);
            CreateCallback?.Invoke();
        }
        else
        {
            await RepositoryReference.EditAsync(Model);
            await EditCallback.InvokeAsync(true);
        }

        Hide();
    }

    protected void HandleSelection(IFileListEntry[]? files)
    {
        selectedFiles = files ?? Array.Empty<IFileListEntry>();
    }

    #endregion
}
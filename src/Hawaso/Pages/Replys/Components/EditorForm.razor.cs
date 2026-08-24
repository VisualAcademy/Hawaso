using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Replys.Components;

public partial class EditorForm
{
    #region Fields

    private string parentId = string.Empty;

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
    public Reply Model { get; set; } = new Reply();

    public Reply ModelEdit { get; set; } = new Reply();

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
    public IReplyRepository RepositoryReference { get; set; } = default!;

    [Inject]
    public IFileStorageManager FileStorageManagerReference { get; set; } = default!;

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// 넘어온 Model 값을 수정 전용 ModelEdit에 담기
    /// </summary>
    protected override void OnParametersSet()
    {
        ModelEdit = new Reply
        {
            Id = Model.Id,
            Name = Model.Name,
            Title = Model.Title,
            Content = Model.Content
        };

        // ParentId가 넘어온 값이 있으면,
        // 즉 0이 아니면 ParentId 드롭다운 리스트 기본값 선택
        parentId = Model.ParentId.ToString();

        if (parentId == "0")
        {
            parentId = string.Empty;
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 생성 또는 수정 버튼 클릭 이벤트 처리기
    /// </summary>
    protected async Task CreateOrEditClick()
    {
        // 변경 내용 저장
        Model.Name = ModelEdit.Name;
        Model.Title = ModelEdit.Title;
        Model.Content = ModelEdit.Content;

        #region 파일 업로드 관련 추가 코드 영역

        if (selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();

            if (file is not null)
            {
                string fileName = file.Name;
                int fileSize = Convert.ToInt32(file.Size);

                // [A] byte[] 형태
                // var ms = new MemoryStream();
                // await file.Data.CopyToAsync(ms);
                // await FileStorageManager.ReplyAsync(
                //     ms.ToArray(),
                //     file.Name,
                //     "",
                //     true);

                // [B] Stream 형태
                await FileStorageManagerReference.UploadAsync(
                    file.Data,
                    file.Name,
                    string.Empty,
                    true);

                Model.FileName = fileName;
                Model.FileSize = fileSize;
            }
        }

        #endregion

        if (!int.TryParse(parentId, out int newParentId))
        {
            newParentId = 0;
        }

        Model.ParentId = newParentId;
        Model.ParentKey = ParentKey;

        if (Model.Id == 0)
        {
            // Create
            await RepositoryReference.AddAsync(Model);
            CreateCallback?.Invoke();
        }
        else
        {
            // Edit
            await RepositoryReference.EditAsync(Model);
            await EditCallback.InvokeAsync(true);
        }
    }

    /// <summary>
    /// 파일 첨부 이벤트 처리기
    /// </summary>
    protected void HandleSelection(IFileListEntry[] files)
    {
        selectedFiles = files ?? Array.Empty<IFileListEntry>();
    }

    #endregion
}
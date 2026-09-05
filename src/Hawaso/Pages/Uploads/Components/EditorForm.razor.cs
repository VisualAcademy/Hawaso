using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Uploads.Components;

public partial class EditorForm
{
    #region Fields

    /// <summary>
    /// 모달 다이얼로그를 표시할건지 여부
    /// </summary>
    public bool IsShow { get; set; }

    private string parentId = string.Empty;

    /// <summary>
    /// 부모(카테고리) 리스트
    /// </summary>
    protected int[] parentIds = { 1, 2, 3 };

    /// <summary>
    /// 첨부 파일 리스트 보관
    /// </summary>
    private IFileListEntry[] selectedFiles = Array.Empty<IFileListEntry>();

    #endregion

    #region Parameters

    [Parameter]
    public string ParentKey { get; set; } = string.Empty;

    /// <summary>
    /// 폼의 제목 영역
    /// </summary>
    [Parameter]
    public RenderFragment? EditorFormTitle { get; set; }

    /// <summary>
    /// 넘어온 모델 개체
    /// </summary>
    [Parameter]
    public Upload Model { get; set; } = new();

    /// <summary>
    /// 부모 컴포넌트에게 생성(Create)이 완료되었다고 보고하는 목적으로
    /// 부모 컴포넌트에게 알림
    /// </summary>
    [Parameter]
    public Action? CreateCallback { get; set; }

    /// <summary>
    /// 부모 컴포넌트에게 수정(Edit)이 완료되었다고 보고하는 목적으로
    /// 부모 컴포넌트에게 알림
    /// </summary>
    [Parameter]
    public EventCallback<bool> EditCallback { get; set; }

    #endregion

    #region Injectors

    /// <summary>
    /// 리포지토리 클래스에 대한 참조
    /// </summary>
    [Inject]
    public IUploadRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public IFileStorageManager FileStorageManager { get; set; } = default!;

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

    #region Lifecycle Methods

    protected override void OnParametersSet()
    {
        parentId = Model.ParentId is > 0
            ? Model.ParentId.Value.ToString()
            : string.Empty;

        // 모달이 다른 항목으로 다시 열릴 때
        // 이전 파일 선택 상태를 초기화
        selectedFiles = Array.Empty<IFileListEntry>();
    }

    #endregion

    #region Event Handlers

    protected async Task CreateOrEditClick()
    {
        #region 파일 업로드 관련 추가 코드 영역

        if (selectedFiles.Length > 0)
        {
            var file = selectedFiles.FirstOrDefault();

            if (file != null)
            {
                string fileName = file.Name;
                int fileSize = Convert.ToInt32(file.Size);

                // Stream 형태로 파일 업로드
                fileName = await FileStorageManager.UploadAsync(
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
            await UploadRepositoryAsyncReference.AddAsync(Model);
            CreateCallback?.Invoke();
        }
        else
        {
            // Edit
            await UploadRepositoryAsyncReference.EditAsync(Model);
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
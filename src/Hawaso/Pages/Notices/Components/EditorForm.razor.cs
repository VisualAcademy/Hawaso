using Microsoft.AspNetCore.Components;
using NoticeApp.Models;

namespace Hawaso.Pages.Notices.Components;

public partial class EditorForm
{
    #region Fields

    private string parentId = string.Empty;

    protected int[] parentIds = { 1, 2, 3 };

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
    public Notice Model { get; set; } = new();

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
    public INoticeRepository RepositoryAsyncReference { get; set; } = default!;

    #endregion

    #region Lifecycle Methods

    protected override void OnParametersSet()
    {
        // ParentId가 0이 아니면 드롭다운의 기본값으로 선택
        parentId = Model.ParentId.ToString();

        if (parentId == "0")
        {
            parentId = string.Empty;
        }
    }

    #endregion

    #region Event Handlers

    protected async Task CreateOrEditClick()
    {
        if (!int.TryParse(parentId, out int newParentId))
        {
            newParentId = 0;
        }

        Model.ParentId = newParentId;

        if (Model.Id == 0)
        {
            // Create
            await RepositoryAsyncReference.AddAsync(Model);
            CreateCallback?.Invoke();
        }
        else
        {
            // Edit
            await RepositoryAsyncReference.EditAsync(Model);
            await EditCallback.InvokeAsync(true);
        }
    }

    #endregion
}
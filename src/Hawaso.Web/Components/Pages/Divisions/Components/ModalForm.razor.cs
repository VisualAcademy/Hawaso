using Microsoft.AspNetCore.Components;
using System;
using Azunt.DivisionManagement;

namespace Azunt.Web.Pages.Divisions.Components;

public partial class ModalForm : ComponentBase
{
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
    public void Show() => IsShow = true; // 현재 인라인 모달 폼 보이기

    /// <summary>
    /// 폼 닫기
    /// </summary>
    public void Hide()
    {
        IsShow = false;
        StateHasChanged(); // 추가!!
    }
    #endregion

    #region Parameters
    [Parameter]
    public string UserName { get; set; } = "";

    /// <summary>
    /// 폼의 제목 영역
    /// </summary>
    [Parameter]
    public RenderFragment EditorFormTitle { get; set; } = null!; // null이 아닌 RenderFragment로 초기화

    /// <summary>
    /// 넘어온 모델 개체 
    /// </summary>
    [Parameter]
    public Division ModelSender { get; set; } = null!; // null이 아닌 Division으로 초기화

    public Division ModelEdit { get; set; } = null!; // null이 아닌 Division으로 초기화

    #region Lifecycle Methods
    // 넘어온 Model 값을 수정 전용 ModelEdit에 담기 
    protected override void OnParametersSet()
    {
        if (ModelSender != null)
        {
            ModelEdit = new Division
            {
                Id = ModelSender.Id,
                Name = ModelSender.Name,
                Active = ModelSender.Active,
                CreatedAt = ModelSender.CreatedAt,
                CreatedBy = ModelSender.CreatedBy
                // 필요한 필드 더 복사
            };
        }
        else
        {
            ModelEdit = new Division();
        }
    }
    #endregion

    /// <summary>
    /// 부모 컴포넌트에게 생성(Create)이 완료되었다고 보고하는 목적으로 부모 컴포넌트에게 알림
    /// 학습 목적으로 Action 대리자 사용
    /// </summary>
    [Parameter]
    public Action CreateCallback { get; set; } = null!; // null이 아닌 Action으로 초기화

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
    public IDivisionRepository RepositoryReference { get; set; } = null!;

    #endregion

    #region Event Handlers
    protected async void CreateOrEditClick()
    {
        // 변경 내용 저장
        ModelSender.Active = true;
        ModelSender.Name = ModelEdit.Name;
        ModelSender.CreatedBy = UserName ?? "Anonymous";

        if (ModelSender.Id == 0)
        {
            // Create
            ModelSender.CreatedAt = DateTime.UtcNow;
            await RepositoryReference.AddAsync(ModelSender);
            CreateCallback?.Invoke();
        }
        else
        {
            // Edit
            await RepositoryReference.UpdateAsync(ModelSender);
            await EditCallback.InvokeAsync(true);
        }
    }
    #endregion
}
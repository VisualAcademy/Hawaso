using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Departments;

namespace VisualAcademy.Pages.Departments.Components;

public partial class ModalForm
{
    #region Properties

    /// <summary>
    /// (글쓰기/글수정) 모달 다이얼로그 표시 여부
    /// </summary>
    public bool IsShow { get; set; }

    /// <summary>
    /// 수정 작업에 사용할 임시 모델
    /// </summary>
    public DepartmentModel ModelEdit { get; set; } = new();

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

    [Parameter]
    public string? UserName { get; set; }

    /// <summary>
    /// 폼의 제목 영역
    /// </summary>
    [Parameter]
    public RenderFragment? EditorFormTitle { get; set; }

    /// <summary>
    /// 넘어온 모델 개체
    /// </summary>
    [Parameter]
    public DepartmentModel ModelSender { get; set; } = new();

    /// <summary>
    /// 부모 컴포넌트에게 생성(Create)이 완료되었다고 알림
    /// 학습 목적으로 Action 대리자 사용
    /// </summary>
    [Parameter]
    public Action? CreateCallback { get; set; }

    /// <summary>
    /// 부모 컴포넌트에게 수정(Edit)이 완료되었다고 알림
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
    public IDepartmentRepository RepositoryReference { get; set; } = default!;

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// 넘어온 Model 값을 수정 전용 ModelEdit에 복사
    /// </summary>
    protected override void OnParametersSet()
    {
        ModelEdit = new DepartmentModel
        {
            Id = ModelSender.Id,
            Name = ModelSender.Name ?? string.Empty
        };
    }

    #endregion

    #region Event Handlers

    protected async Task CreateOrEditClick()
    {
        // 변경 내용 저장
        ModelSender.Active = true;
        ModelSender.Name = ModelEdit.Name ?? string.Empty;
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
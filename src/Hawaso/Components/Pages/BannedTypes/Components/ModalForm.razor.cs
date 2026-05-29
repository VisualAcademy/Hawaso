using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.BannedTypes;

namespace VisualAcademy.Components.Pages.BannedTypes.Components;

public partial class ModalForm
{
    #region Properties

    /// <summary>
    /// (글쓰기/글수정) 모달 다이얼로그를 표시할지 여부
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

    [Parameter]
    public string UserName { get; set; } = "Anonymous";

    /// <summary>
    /// 폼의 제목 영역
    /// </summary>
    [Parameter]
    public RenderFragment? EditorFormTitle { get; set; }

    /// <summary>
    /// 넘어온 모델 개체
    /// </summary>
    [Parameter]
    public BannedTypeModel ModelSender { get; set; } = new();

    public BannedTypeModel ModelEdit { get; set; } = new();

    /// <summary>
    /// 부모 컴포넌트에게 생성(Create)이 완료되었다고 보고
    /// </summary>
    [Parameter]
    public Action? CreateCallback { get; set; }

    /// <summary>
    /// 부모 컴포넌트에게 수정(Edit)이 완료되었다고 보고
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
    public IBannedTypeRepository RepositoryReference { get; set; } = default!;

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// 넘어온 Model 값을 수정 전용 ModelEdit에 복사
    /// </summary>
    protected override void OnParametersSet()
    {
        ModelEdit = new BannedTypeModel
        {
            Id = ModelSender.Id,
            Name = ModelSender.Name
        };
    }

    #endregion

    #region Event Handlers

    protected async Task CreateOrEditClick()
    {
        ModelSender.Active = true;
        ModelSender.Name = ModelEdit.Name;
        ModelSender.CreatedBy = string.IsNullOrWhiteSpace(UserName)
            ? "Anonymous"
            : UserName;

        if (ModelSender.Id == 0)
        {
            ModelSender.CreatedAt = DateTime.UtcNow;

            await RepositoryReference.AddAsync(ModelSender);

            CreateCallback?.Invoke();
        }
        else
        {
            await RepositoryReference.UpdateAsync(ModelSender);

            await EditCallback.InvokeAsync(true);
        }
    }

    #endregion
}
using Hawaso.Web.Components.Pages.DivisionPages.Models;
using Microsoft.AspNetCore.Components;

namespace Hawaso.Web.Components.Pages.DivisionPages.Components;

public partial class ModalForm : ComponentBase
{
    #region Parameters
    /// <summary>
    /// (글쓰기/글수정) 모달 다이얼로그를 표시할건지 여부 
    /// </summary>
    public bool IsShow { get; set; } = false;

    /// <summary>
    /// Bootstrap 5 사용 여부 (기본값: true)
    /// </summary>
    [Parameter]
    public bool UseBootstrap5 { get; set; } = true;

    /// <summary>
    /// 부모 컴포넌트에게 생성(Create)이 완료되었다고 보고하는 콜백
    /// </summary>
    [Parameter]
    public Action CreateCallback { get; set; }

    /// <summary>
    /// 부모 컴포넌트에게 수정(Edit)이 완료되었다고 보고하는 콜백
    /// </summary>
    [Parameter]
    public EventCallback<bool> EditCallback { get; set; }

    /// <summary>
    /// 부모 키
    /// </summary>
    [Parameter]
    public string ParentKey { get; set; } = "";

    /// <summary>
    /// 사용자 이름
    /// </summary>
    [Parameter]
    public string UserName { get; set; }

    /// <summary>
    /// 폼의 제목 영역
    /// </summary>
    [Parameter]
    public RenderFragment EditorFormTitle { get; set; }

    /// <summary>
    /// 넘어온 모델 개체 
    /// </summary>
    [Parameter]
    public DivisionModel ModelSender { get; set; }

    public DivisionModel ModelEdit { get; set; }
    #endregion

    #region Injectors
    /// <summary>
    /// 리포지토리 클래스에 대한 참조 
    /// </summary>
    [Inject]
    public IDivisionRepository RepositoryReference { get; set; }
    #endregion

    #region Lifecycle Methods
    /// <summary>
    /// 넘어온 Model 값을 수정 전용 ModelEdit에 복사
    /// </summary>
    protected override void OnParametersSet()
    {
        ModelEdit = new DivisionModel
        {
            Id = ModelSender.Id,
            Name = ModelSender.Name
            // 더 많은 정보 추가 가능...
        };
    }
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

        // 모달 닫기
        Hide();
    }
    #endregion
}

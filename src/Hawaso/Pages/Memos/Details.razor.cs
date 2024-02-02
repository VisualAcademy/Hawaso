using Microsoft.AspNetCore.Components;

namespace VisualAcademy.Pages.Memos;

public partial class Details
{
    #region Parameters
    [Parameter]
    public int Id { get; set; }
    #endregion

    #region Injectors
    [Inject]
    public IMemoRepository RepositoryReference { get; set; }
    #endregion

    #region Properties
    // MVC에서 Controller에서 View로 Model 개체로 데이터 전송하는 것처럼, 코드 비하인드에서 컴포넌트로 모델 값 전송
    public Memo Model { get; set; } = new Memo();

    public string Content { get; set; } = "";
    #endregion

    #region Lifecycle Methods
    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        Model = await RepositoryReference.GetByIdAsync(Id);
        Content = Dul.HtmlUtility.EncodeWithTabAndSpace(Model.Content);
    }
    #endregion
}

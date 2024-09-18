using Microsoft.AspNetCore.Components;

namespace VisualAcademy.Pages.Purges.Components;

public partial class SearchBox : IDisposable
{
    #region Fields
    private string searchQuery; // 검색어 담을 그릇 
    private System.Timers.Timer debounceTimer; // 디바운스 타임: 응답 대기 시간(300밀리초)
    #endregion

    #region Parameters
    #region 부모에서 전달된 기타 특성들을 모두 받아서 사용
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object> AdditionalAttributes { get; set; }
    #endregion

    // 자식 컴포넌트에서 발생한 정보를 부모 컴포넌트에게 전달
    [Parameter]
    public EventCallback<string> SearchQueryChanged { get; set; }

    [Parameter]
    public int Debounce { get; set; } = 300;
    #endregion

    #region Properties
    /// <summary>
    /// 부모(외부) 컴포넌트에 공개할 속성 이름
    /// </summary>
    public string SearchQuery
    {
        get => searchQuery;
        set
        {
            searchQuery = value;

            #region 타이머 가동
            debounceTimer.Stop(); // 텍스트박스에 값을 입력하는 동안 타이머 중지
            debounceTimer.Start(); // 타이머 실행(300밀리초 후에 딱 한 번 실행) 
            #endregion
        }
    }
    #endregion

    #region Lifecycle Methods
    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override void OnInitialized()
    {
        debounceTimer = new System.Timers.Timer();
        debounceTimer.Interval = Debounce;
        debounceTimer.AutoReset = false; // 딱 한번 실행 
        debounceTimer.Elapsed += SearchHandler;
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// 검색 버튼 클릭했을 때 실행
    /// </summary>
    protected void Search()
    {
        SearchQueryChanged.InvokeAsync(SearchQuery); // 부모의 메서드에 검색어 전달
    }

    /// <summary>
    /// 타이머에 의해서 실행
    /// </summary>
    protected async void SearchHandler(object source, ElapsedEventArgs e)
    {
        await InvokeAsync(() => SearchQueryChanged.InvokeAsync(SearchQuery)); // 부모의 메서드에 검색어 전달
    }
    #endregion

    #region Public Methods
    public void Dispose()
    {
        debounceTimer.Dispose(); // 타이머 개체 소멸 
    }
    #endregion
}

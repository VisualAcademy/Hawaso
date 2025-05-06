using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Azunt.Web.Components.Pages.Files.Components;

public partial class SearchBox : ComponentBase, IDisposable
{
    #region Fields
    private string searchQuery = "";
    private System.Timers.Timer? debounceTimer;
    #endregion

    #region Parameters

    /// <summary>
    /// 추가 HTML 속성 (placeholder 등) 처리
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// 부모 컴포넌트로 검색어 전달
    /// </summary>
    [Parameter]
    public EventCallback<string> SearchQueryChanged { get; set; }

    /// <summary>
    /// 디바운스 시간 (기본값: 300ms)
    /// </summary>
    [Parameter]
    public int Debounce { get; set; } = 300;

    #endregion

    #region Properties

    /// <summary>
    /// 검색어 바인딩 속성 (입력 시 디바운스 적용)
    /// </summary>
    public string SearchQuery
    {
        get => searchQuery;
        set
        {
            searchQuery = value;
            debounceTimer?.Stop();    // 입력 중이면 기존 타이머 중지
            debounceTimer?.Start();   // 새 타이머 시작 (입력 완료 후 실행)
        }
    }

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// 컴포넌트 초기화 시 디바운스 타이머 구성
    /// </summary>
    protected override void OnInitialized()
    {
        debounceTimer = new System.Timers.Timer
        {
            Interval = Debounce,
            AutoReset = false // 한 번만 실행되도록 설정
        };
        debounceTimer.Elapsed += SearchHandler!;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Search 버튼 직접 클릭 시 즉시 검색 실행
    /// </summary>
    protected void Search()
    {
        SearchQueryChanged.InvokeAsync(SearchQuery);
    }

    /// <summary>
    /// 디바운스 타이머 종료 시 이벤트 발생
    /// </summary>
    protected async void SearchHandler(object source, ElapsedEventArgs e)
    {
        await InvokeAsync(() => SearchQueryChanged.InvokeAsync(SearchQuery));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 리소스 해제
    /// </summary>
    public void Dispose()
    {
        debounceTimer?.Dispose();
    }

    #endregion
}
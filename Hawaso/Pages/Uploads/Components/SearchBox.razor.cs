using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Hawaso.Pages.Uploads.Components
{
    public partial class SearchBox : IDisposable
    {
        private string searchQuery;
        private Timer debounceTimer;

        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                searchQuery = value;
                debounceTimer.Stop(); // 텍스트박스에 값을 입력하는 동안 타이머 중지
                debounceTimer.Start(); // 타이머 실행(300밀리초 후에 딱 한 번 실행)
            }
        }

        [Parameter(CaptureUnmatchedValues = true)]
        public IDictionary<string, object> AdditionalAttributes { get; set; }

        // 자식 컴포넌트에서 발생한 정보를 부모 컴포넌트에게 전달
        [Parameter]
        public EventCallback<string> SearchQueryChanged { get; set; }

        [Parameter]
        public int Debounce { get; set; } = 300;

        protected override void OnInitialized()
        {
            debounceTimer = new Timer();
            debounceTimer.Interval = Debounce;
            debounceTimer.AutoReset = false;
            debounceTimer.Elapsed += SearchHandler;
        }

        protected void Search()
        {
            SearchQueryChanged.InvokeAsync(SearchQuery); // 부모의 메서드에 검색어 전달
        }

        protected async void SearchHandler(object source, ElapsedEventArgs e)
        {
            await InvokeAsync(() => SearchQueryChanged.InvokeAsync(SearchQuery)); // 부모의 메서드에 검색어 전달
        }

        public void Dispose()
        {
            debounceTimer.Dispose();
        }
    }
}

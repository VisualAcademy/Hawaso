using Microsoft.AspNetCore.Components;
using NoticeApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Notices
{
    public partial class Index
    {
        [Inject]
        public INoticeRepository NoticeRepositoryAsyncReference { get; set; }

        [Inject]
        public NavigationManager NavigationManagerReference { get; set; }

        protected List<Notice> models;

        protected DulPager.DulPagerBase pager = new DulPager.DulPagerBase() 
        { 
            PageNumber = 1,
            PageIndex = 0,
            PageSize = 2,
            PagerButtonCount = 5
        };

        protected override async Task OnInitializedAsync()
        {
            if (this.searchQuery != "")
            {
                await DisplayData();
            }
            else
            {
                await SearchData(); 
            }
        }

        /// <summary>
        /// NoticeApp - UploadApp - ReplyApp을 거쳐 여기 코드가 더 정리됩니다.
        /// </summary>
        /// <returns></returns>
        private async Task DisplayData()
        {
            //await Task.Delay(3000);
            var resultsSet = await NoticeRepositoryAsyncReference.GetAllAsync(pager.PageIndex, pager.PageSize);
            pager.RecordCount = resultsSet.TotalRecords;
            models = resultsSet.Records.ToList();
        }

        private async Task SearchData()
        {
            var resultsSet = await NoticeRepositoryAsyncReference.SearchAllAsync(pager.PageIndex, pager.PageSize, this.searchQuery);
            pager.RecordCount = resultsSet.TotalRecords;
            models = resultsSet.Records.ToList();
        }

        protected void NameClick(int id)
        {
            //[?] Id 값을 가지고 상세보기 페이지로 이동
            NavigationManagerReference.NavigateTo($"/Notices/Details/{id}");
        }

        protected async void PageIndexChanged(int pageIndex)
        {
            pager.PageIndex = pageIndex;
            pager.PageNumber = pageIndex + 1;

            if (this.searchQuery == "")
            {
                await DisplayData();
            }
            else
            {
                await SearchData();
            }

            StateHasChanged();
        }

        private string searchQuery = "";

        protected async void Search(string query)
        {
            this.searchQuery = query;

            await SearchData();

            StateHasChanged();
        }
    }
}

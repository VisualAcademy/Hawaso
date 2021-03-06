using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zero.Models;

namespace Hawaso.Pages.BriefingLogs
{
    public partial class Index
    {
        [Inject]
        public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; }

        [Inject]
        public NavigationManager NavigationManagerReference { get; set; }

        protected List<BriefingLog> models;

        protected DulPager.DulPagerBase pager = new DulPager.DulPagerBase()
        {
            PageNumber = 1,
            PageIndex = 0,
            PageSize = 10,
            PagerButtonCount = 5
        };

        protected override async Task OnInitializedAsync()
        {
            await DisplayData();
        }

        private async Task DisplayData()
        {
            var articleSet = await UploadRepositoryAsyncReference.GetArticles<int>(pager.PageIndex, pager.PageSize, "", this.searchQuery, this.sortOrder, 0);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();

            StateHasChanged();
        }

        protected void NameClick(int id)
        {
            NavigationManagerReference.NavigateTo($"/BriefingLogs/Details/{id}");
        }

        protected async void PageIndexChanged(int pageIndex)
        {
            pager.PageIndex = pageIndex;
            pager.PageNumber = pageIndex + 1;

            await DisplayData();

            StateHasChanged();
        }

        #region Search
        private string searchQuery = "";

        protected async void Search(string query)
        {
            pager.PageIndex = 0;

            this.searchQuery = query;

            await DisplayData();

            StateHasChanged();
        }
        #endregion

        #region Sorting
        private string sortOrder = "";

        protected async void SortByName()
        {
            if (sortOrder == "")
            {
                sortOrder = "Name";
            }
            else if (sortOrder == "Name")
            {
                sortOrder = "NameDesc";
            }
            else
            {
                sortOrder = "";
            }

            await DisplayData();
        }

        protected async void SortByTitle()
        {
            if (sortOrder == "")
            {
                sortOrder = "Title";
            }
            else if (sortOrder == "Title")
            {
                sortOrder = "TitleDesc";
            }
            else
            {
                sortOrder = "";
            }

            await DisplayData();
        }
        #endregion
    }
}

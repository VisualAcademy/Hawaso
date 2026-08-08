using DotNetSaleCore.Models;
using DulPager;
using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.Products
{
    public partial class Index
    {
        [Inject]
        public IProductRepositoryAsync ProductRepositoryAsync { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        private DulPagerBase pager = new DulPagerBase()
        {
            PageNumber = 1,
            PageIndex = 0,
            PageSize = 3,
            PagerButtonCount = 5
        };

        private List<Product> Products = new();

        protected override async Task OnInitializedAsync()
        {
            await DisplayData();
        }

        private async Task DisplayData()
        {
            var articleSet = await ProductRepositoryAsync.GetAllAsync(
                pager.PageIndex,
                pager.PageSize);

            pager.RecordCount = articleSet.TotalRecords;
            Products = articleSet.Records.ToList();
        }

        private async void PageIndexChanged(int pageIndex)
        {
            pager.PageIndex = pageIndex;
            pager.PageNumber = pageIndex + 1;

            await DisplayData();

            StateHasChanged();
        }
    }
}
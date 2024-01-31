using DotNetSaleCore.Models;
using DulPager;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Products
{
    public partial class Index
    {
        [Inject]
        public IProductRepositoryAsync ProductRepositoryAsync { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private DulPagerBase pager = new DulPagerBase()
        {
            PageNumber = 1,
            PageIndex = 0,
            PageSize = 3,
            PagerButtonCount = 5
        };

        private List<Product> Products; 

        protected override async Task OnInitializedAsync()
        {
            await DisplayData();
        }

        private async Task DisplayData()
        {
            var articleSet = await ProductRepositoryAsync.GetAllAsync(pager.PageIndex, pager.PageSize);
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

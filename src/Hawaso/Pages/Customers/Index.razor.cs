using DotNetSaleCore.Models;
using DulPager;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Customers
{
    public partial class Index
    {
        [Inject]
        public ICustomerRepository CustomerRepositoryAsync { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private DulPagerBase pager = new DulPagerBase()
        {
            PageNumber = 1,
            PageIndex = 0,
            PageSize = 3,
            PagerButtonCount = 5
        };

        private List<Customer> customers; 

        protected override async Task OnInitializedAsync()
        {
            await DisplayData();
        }

        private async Task DisplayData()
        {
            //await Task.Delay(3000);
            var articleSet = await CustomerRepositoryAsync.GetAllAsync(pager.PageIndex, pager.PageSize);
            pager.RecordCount = articleSet.TotalRecords;
            customers = articleSet.Records.ToList();
        }

        private async void PageIndexChanged(int pageIndex)
        {
            pager.PageIndex = pageIndex;
            pager.PageNumber = pageIndex + 1;

            await DisplayData();

            StateHasChanged();
        }

        private void btnCustomerName_Click(int customerId)
        {
            NavigationManager.NavigateTo($"/Customers/Details/{customerId}");
        }
    }
}

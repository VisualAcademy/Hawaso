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
        public ICustomerRepository CustomerRepositoryAsync { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        private readonly DulPagerBase pager = new()
        {
            PageNumber = 1,
            PageIndex = 0,
            PageSize = 3,
            PagerButtonCount = 5
        };

        private List<Customer>? customers;

        protected override async Task OnInitializedAsync()
        {
            await DisplayData();
        }

        private async Task DisplayData()
        {
            var articleSet = await CustomerRepositoryAsync.GetAllAsync(
                pager.PageIndex,
                pager.PageSize);

            pager.RecordCount = articleSet.TotalRecords;
            customers = articleSet.Records?.ToList() ?? new List<Customer>();
        }

        private async Task PageIndexChanged(int pageIndex)
        {
            pager.PageIndex = pageIndex;
            pager.PageNumber = pageIndex + 1;

            await DisplayData();
        }

        private void btnCustomerName_Click(int customerId)
        {
            NavigationManager.NavigateTo($"/Customers/Details/{customerId}");
        }
    }
}
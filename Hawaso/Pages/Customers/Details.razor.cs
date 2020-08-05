using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Hawaso.Pages.Customers
{
    public partial class Details
    {
        [Parameter]
        public int CustomerId { get; set; }

        [Inject]
        public ICustomerRepository CustomerRepositoryReference { get; set; }

        private Customer customer = new Customer();

        protected override async Task OnInitializedAsync()
        {
            customer = await CustomerRepositoryReference.GetByIdAsync(CustomerId);
        }
    }
}

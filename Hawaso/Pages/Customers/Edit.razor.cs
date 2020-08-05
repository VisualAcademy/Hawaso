using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Hawaso.Pages.Customers
{
    public partial class Edit
    {
        [Parameter]
        public int CustomerId { get; set; }

        [Inject]
        public ICustomerRepository CustomerRepositoryAsync { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private Customer customer = new Customer();

        protected override async Task OnInitializedAsync()
        {
            customer = await CustomerRepositoryAsync.GetByIdAsync(CustomerId);
        }

        private string[] genders = { "Male", "Female" };

        protected async Task btnEdit_Click()
        {
            customer.Modified = DateTime.Now;
            await CustomerRepositoryAsync.EditAsync(customer);
            NavigationManager.NavigateTo($"/Customers/Details/{CustomerId}");
        }
    }
}

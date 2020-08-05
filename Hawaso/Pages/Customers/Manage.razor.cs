using Hawaso.Pages.Customers.Components;
using DotNetSaleCore.Models;
using DulPager;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Customers
{
    public partial class Manage
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

        public string EditorFormTitle { get; set; } = "ADD";

        public Customer Customer { get; set; } = new Customer();

        public CustomerEditorForm CustomerEditorForm { get; set; }

        public CustomerDeleteDialog CustomerDeleteDialog { get; set; }

        public bool IsInlineDialogShow { get; set; }

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

        protected void btnCreate_Click()
        {
            EditorFormTitle = "ADD";
            Customer = new Customer();

            CustomerEditorForm.Show(); 
        }

        protected async void SaveOrUpdated()
        {
            CustomerEditorForm.Close();

            await DisplayData();

            StateHasChanged();
        }

        protected void EditBy(Customer customer)
        {
            EditorFormTitle = "EDIT";
            Customer = customer; 

            CustomerEditorForm.Show();
        }

        protected void DeleteBy(Customer customer)
        {
            Customer = customer;
            CustomerDeleteDialog.Show();
        }

        protected async void btnDelete_Click()
        {
            await CustomerRepositoryAsync.DeleteAsync(Customer.CustomerId);

            CustomerDeleteDialog.Close();

            Customer = new Customer(); 

            await DisplayData();

            StateHasChanged();
        }

        protected void ToggleBy(Customer customer)
        {
            Customer = customer;

            IsInlineDialogShow = true;
        }

        protected async void btnToggleGender_Click()
        {
            Customer.Gender = (Customer.Gender == "Male" ? "Female" : "Male");
            await CustomerRepositoryAsync.EditAsync(Customer);

            await DisplayData();

            IsInlineDialogShow = false;
            StateHasChanged();
        }

        protected void btnClose_Click()
        {
            IsInlineDialogShow = false;
            Customer = new Customer(); 
        }
    }
}

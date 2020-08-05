using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Hawaso.Pages.Customers
{
    public partial class Delete
    {
        [Parameter]
        public int CustomerId { get; set; }

        [Inject]
        public ICustomerRepository CustomerRepositoryAsync { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private Customer customer = new Customer();

        protected override async Task OnInitializedAsync()
        {
            customer = await CustomerRepositoryAsync.GetByIdAsync(CustomerId);
        }

        protected async void btnDelete_Click()
        {
            bool isDelete = await JSRuntime.InvokeAsync<bool>("confirm", $"{CustomerId}번 고객 정보를 정말로 삭제하시겠습니까?");
            if (isDelete)
            {
                await CustomerRepositoryAsync.DeleteAsync(CustomerId);
                NavigationManager.NavigateTo("/Customers"); 
            }
            else
            {
                await JSRuntime.InvokeAsync<object>("alert", "취소되었습니다.");
            }
        }
    }
}

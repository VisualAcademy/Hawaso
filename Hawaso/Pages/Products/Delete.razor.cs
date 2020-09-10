using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Hawaso.Pages.Products
{
    public partial class Delete
    {
        [Parameter]
        public int ProductId { get; set; }

        [Inject]
        public IProductRepositoryAsync ProductRepositoryAsync { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private Product Product = new Product();

        protected override async Task OnInitializedAsync()
        {
            Product = await ProductRepositoryAsync.GetByIdAsync(ProductId);
        }

        protected async void btnDelete_Click()
        {
            bool isDelete = await JSRuntime.InvokeAsync<bool>("confirm", $"{ProductId}번 상품 정보를 정말로 삭제하시겠습니까?");
            if (isDelete)
            {
                await ProductRepositoryAsync.DeleteAsync(ProductId);
                NavigationManager.NavigateTo("/Products"); 
            }
            else
            {
                await JSRuntime.InvokeAsync<object>("alert", "취소되었습니다.");
            }
        }
    }
}

using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Hawaso.Pages.Products
{
    public partial class Details
    {
        [Parameter]
        public int ProductId { get; set; }

        [Inject]
        public IProductRepositoryAsync ProductRepositoryAsync { get; set; }

        private Product Product = new Product();

        protected override async Task OnInitializedAsync()
        {
            Product = await ProductRepositoryAsync.GetByIdAsync(ProductId);
        }
    }
}

using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Hawaso.Pages.Products
{
    public partial class Edit
    {
        [Parameter]
        public int ProductId { get; set; }

        [Inject]
        public IProductRepositoryAsync ProductRepositoryAsync { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ICategoryRepository CategoryRepositoryAsync { get; set; }

        private Product Product = new Product();

        public string CategoryId { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();

        protected override async Task OnInitializedAsync()
        {
            Product = await ProductRepositoryAsync.GetByIdAsync(ProductId);
            Categories = await CategoryRepositoryAsync.GetAllAsync();
            CategoryId = Product.CategoryId.ToString();
        }

        private string[] genders = { "Male", "Female" };

        protected async Task btnEdit_Click()
        {
            Product.CategoryId = Convert.ToInt32(CategoryId); 
            await ProductRepositoryAsync.EditAsync(Product);
            NavigationManager.NavigateTo($"/Products/Details/{ProductId}");
        }
    }
}

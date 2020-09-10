using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hawaso.Pages.Products
{
    public partial class Create
    {
        [Inject]
        public IProductRepositoryAsync ProductRepositoryAsync { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ICategoryRepository CategoryRepositoryAsync { get; set; }

        private Product Product = new Product();

        //private string[] genders = { "Male", "Female" };

        public string CategoryId { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();

        protected async Task btnSubmit_Click()
        {
            Product.CategoryId = Convert.ToInt32(CategoryId); // 선택한 카테고리
            await ProductRepositoryAsync.AddAsync(Product);
            NavigationManager.NavigateTo("/Products");
        }

        protected override async Task OnInitializedAsync()
        {
            Categories = await CategoryRepositoryAsync.GetAllAsync(); 
        }
    }
}

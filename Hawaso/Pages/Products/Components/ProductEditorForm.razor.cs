using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hawaso.Pages.Products.Components
{
    public partial class ProductEditorForm
    {
        [Parameter]
        public RenderFragment EditorFormTitle { get; set; }

        [Parameter]
        public Product Model { get; set; }

        [Parameter]
        public Action SaveOrUpdated { get; set; } // EventCallback<bool> 

        [Parameter]
        public EventCallback<bool> ChangeCallback { get; set; }

        [Inject]
        public IProductRepositoryAsync ProductRepositoryAsync { get; set; }

        [Inject]
        public ICategoryRepository CategoryRepositoryAsync { get; set; }

        public bool IsShow { get; set; }

        public string CategoryId { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();

        public void Show()
        {
            IsShow = true;
        }

        public void Close()
        {
            IsShow = false; 
        }

        protected async void btnSaveOrUpdate_Click()
        {
            Model.CategoryId = Convert.ToInt32(CategoryId); 
            if (Model.ProductId == 0)
            {
                await ProductRepositoryAsync.AddAsync(Model);
                SaveOrUpdated?.Invoke(); 
            }
            else
            {
                await ProductRepositoryAsync.EditAsync(Model); 
                await ChangeCallback.InvokeAsync(true);
            }
            IsShow = false;
        }

        protected override void OnParametersSet()
        {
            CategoryId = Model.CategoryId.ToString(); // 기존 카테고리 설정     
            if (CategoryId == "0")
            {
                CategoryId = ""; // --Select Category--
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Categories = await CategoryRepositoryAsync.GetAllAsync();
        }
    }
}

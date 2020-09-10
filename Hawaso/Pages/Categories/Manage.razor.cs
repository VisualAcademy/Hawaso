using Hawaso.Pages.Categories.Components;
using DotNetSaleCore.Models;
using DulPager;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Categories
{
    public partial class Manage
    {
        [Inject]
        public ICategoryRepository CategoryRepositoryAsync { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private DulPagerBase pager = new DulPagerBase()
        {
            PageNumber = 1,
            PageIndex = 0,
            PageSize = 10,
            PagerButtonCount = 5
        };

        private List<Category> customers;

        public string EditorFormTitle { get; set; } = "ADD";

        public Category Category { get; set; } = new Category();

        public CategoryEditorForm CategoryEditorForm { get; set; }

        public CategoryDeleteDialog CategoryDeleteDialog { get; set; }

        public bool IsInlineDialogShow { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await DisplayData();
        }

        private async Task DisplayData()
        {
            var articleSet = await CategoryRepositoryAsync.GetAllAsync(pager.PageIndex, pager.PageSize);
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

        protected void btnCreate_Click()
        {
            EditorFormTitle = "ADD";
            Category = new Category();

            CategoryEditorForm.Show(); 
        }

        protected async void SaveOrUpdated()
        {
            CategoryEditorForm.Close();

            await DisplayData();

            StateHasChanged();
        }

        protected void EditBy(Category customer)
        {
            EditorFormTitle = "EDIT";
            Category = customer; 

            CategoryEditorForm.Show();
        }

        protected void DeleteBy(Category customer)
        {
            Category = customer;
            CategoryDeleteDialog.Show();
        }

        protected async void btnDelete_Click()
        {
            await CategoryRepositoryAsync.DeleteAsync(Category.CategoryId);

            CategoryDeleteDialog.Close();

            Category = new Category(); 

            await DisplayData();

            StateHasChanged();
        }

        protected void btnClose_Click()
        {
            IsInlineDialogShow = false;
            Category = new Category(); 
        }
    }
}

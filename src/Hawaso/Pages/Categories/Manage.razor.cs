using DotNetSaleCore.Models;
using DulPager;
using Hawaso.Pages.Categories.Components;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Categories
{
    public partial class Manage
    {
        [Inject]
        public ICategoryRepository CategoryRepositoryAsync { get; set; } = default!;

        private readonly DulPagerBase pager = new DulPagerBase
        {
            PageNumber = 1,
            PageIndex = 0,
            PageSize = 10,
            PagerButtonCount = 5
        };

        private List<Category> categories = new List<Category>();

        private CategoryEditorForm? categoryEditorForm;

        private CategoryDeleteDialog? categoryDeleteDialog;

        private bool isLoading = true;

        public string EditorFormTitle { get; set; } = "ADD";

        public Category Category { get; set; } = new Category();

        public bool IsInlineDialogShow { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await DisplayDataAsync();
        }

        private async Task DisplayDataAsync()
        {
            isLoading = true;

            var categorySet = await CategoryRepositoryAsync.GetAllAsync(
                pager.PageIndex,
                pager.PageSize);

            pager.RecordCount = categorySet.TotalRecords;
            categories = categorySet.Records?.ToList() ?? new List<Category>();

            isLoading = false;
        }

        private async Task PageIndexChangedAsync(int pageIndex)
        {
            pager.PageIndex = pageIndex;
            pager.PageNumber = pageIndex + 1;

            await DisplayDataAsync();
        }

        protected void btnCreate_Click()
        {
            EditorFormTitle = "ADD";
            Category = new Category();

            categoryEditorForm?.Show();
        }

        protected async Task SaveOrUpdatedAsync()
        {
            categoryEditorForm?.Close();

            await DisplayDataAsync();
        }

        protected void EditBy(Category category)
        {
            EditorFormTitle = "EDIT";
            Category = category;

            categoryEditorForm?.Show();
        }

        protected void DeleteBy(Category category)
        {
            Category = category;

            categoryDeleteDialog?.Show();
        }

        protected async Task btnDelete_ClickAsync()
        {
            await CategoryRepositoryAsync.DeleteAsync(Category.CategoryId);

            categoryDeleteDialog?.Close();

            Category = new Category();

            await DisplayDataAsync();
        }

        protected void btnClose_Click()
        {
            IsInlineDialogShow = false;
            Category = new Category();
        }
    }
}
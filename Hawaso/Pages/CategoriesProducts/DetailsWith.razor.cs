using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Hawaso.Pages.CategoriesProducts
{
    public partial class DetailsWith
    {
        [Parameter]
        public int CategoryId { get; set; }

        [Inject]
        public ICategoryRepository CategoryRepositoryAsync { get; set; }

        private Category category = new Category();

        protected override async Task OnInitializedAsync()
        {
            category = await CategoryRepositoryAsync.GetByIdAsync(CategoryId);
        }
    }
}

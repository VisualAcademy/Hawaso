using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.CategoriesProducts;

public partial class DetailsWith
{
    #region Parameters

    [Parameter]
    public int CategoryId { get; set; }

    #endregion

    #region Injectors

    [Inject]
    public ICategoryRepository CategoryRepositoryAsync { get; set; } = default!;

    #endregion

    #region Fields

    private Category category = new();

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        category = await CategoryRepositoryAsync.GetByIdAsync(CategoryId);
    }

    #endregion
}
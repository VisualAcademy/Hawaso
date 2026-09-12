using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.Products;

public partial class Edit
{
    #region Parameters

    [Parameter]
    public int ProductId { get; set; }

    #endregion

    #region Injectors

    [Inject]
    public IProductRepositoryAsync ProductRepositoryAsync { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public ICategoryRepository CategoryRepositoryAsync { get; set; } = default!;

    #endregion

    #region Fields

    private Product Product = new();

    private string[] genders = { "Male", "Female" };

    #endregion

    #region Properties

    public string CategoryId { get; set; } = string.Empty;

    public List<Category> Categories { get; set; } = new();

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        Product = await ProductRepositoryAsync.GetByIdAsync(ProductId);

        Categories = await CategoryRepositoryAsync.GetAllAsync();

        CategoryId = Product.CategoryId.ToString();
    }

    #endregion

    #region Event Handlers

    protected async Task btnEdit_Click()
    {
        if (int.TryParse(CategoryId, out int categoryId))
        {
            Product.CategoryId = categoryId;
        }

        await ProductRepositoryAsync.EditAsync(Product);

        NavigationManager.NavigateTo(
            $"/Products/Details/{ProductId}");
    }

    #endregion
}
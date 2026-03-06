using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.Products;

public partial class Create
{
    [Inject]
    public IProductRepositoryAsync ProductRepositoryAsync { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public ICategoryRepository RepositoryReference { get; set; } = default!;

    private Product Product = new Product();

    public string CategoryId { get; set; } = string.Empty;

    public List<Category> Categories { get; set; } = new List<Category>();

    protected async Task btnSubmit_Click()
    {
        Product.CategoryId = Convert.ToInt32(CategoryId);
        await ProductRepositoryAsync.AddAsync(Product);
        NavigationManager.NavigateTo("/Products");
    }

    protected override async Task OnInitializedAsync()
        => Categories = await RepositoryReference.GetAllAsync();
}
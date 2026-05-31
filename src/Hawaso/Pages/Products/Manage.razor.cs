using DotNetSaleCore.Models;
using DulPager;
using Hawaso.Pages.Products.Components;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VisualAcademy.Pages.Products;

public partial class Manage
{
    [Inject]
    public IProductRepositoryAsync ProductRepositoryAsync { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    private readonly DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 3,
        PagerButtonCount = 5
    };

    private List<Product>? Products { get; set; }

    public string EditorFormTitle { get; set; } = "ADD";

    public Product Product { get; set; } = new();

    private ProductEditorForm? ProductEditorForm { get; set; }

    private ProductDeleteDialog? ProductDeleteDialog { get; set; }

    public bool IsInlineDialogShow { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await DisplayData();
    }

    private async Task DisplayData()
    {
        var productSet = await ProductRepositoryAsync.GetAllAsync(
            pager.PageIndex,
            pager.PageSize);

        pager.RecordCount = productSet.TotalRecords;
        Products = productSet.Records?.ToList() ?? new List<Product>();
    }

    private async Task PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();
    }

    private void btnProductName_Click(int productId)
    {
        NavigationManager.NavigateTo($"/Products/Details/{productId}");
    }

    protected void ShowEditorForm()
    {
        EditorFormTitle = "ADD";
        Product = new Product();

        ProductEditorForm?.Show();
    }

    protected async Task SaveOrUpdated()
    {
        ProductEditorForm?.Close();

        await DisplayData();
    }

    protected void EditBy(Product product)
    {
        EditorFormTitle = "EDIT";
        Product = product;

        ProductEditorForm?.Show();
    }

    protected void DeleteBy(Product product)
    {
        Product = product;

        ProductDeleteDialog?.Show();
    }

    protected async Task btnDelete_Click()
    {
        if (Product.ProductId <= 0)
        {
            return;
        }

        await ProductRepositoryAsync.DeleteAsync(Product.ProductId);

        ProductDeleteDialog?.Close();

        Product = new Product();

        await DisplayData();
    }

    protected void ToggleBy(Product product)
    {
        Product = product;
        IsInlineDialogShow = true;
    }

    protected async Task btnToggleAbsence_Click()
    {
        if (Product.ProductId <= 0)
        {
            return;
        }

        Product.Absence = Product.Absence == 0 ? 1 : 0;

        await ProductRepositoryAsync.EditAsync(Product);

        await DisplayData();

        IsInlineDialogShow = false;
    }

    protected void btnClose_Click()
    {
        IsInlineDialogShow = false;
        Product = new Product();
    }
}
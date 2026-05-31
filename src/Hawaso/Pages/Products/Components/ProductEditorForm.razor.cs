using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hawaso.Pages.Products.Components;

public partial class ProductEditorForm
{
    [Parameter]
    public RenderFragment? EditorFormTitle { get; set; }

    [Parameter]
    public Product Model { get; set; } = new();

    [Parameter]
    public EventCallback SaveOrUpdated { get; set; }

    [Inject]
    public IProductRepositoryAsync ProductRepositoryAsync { get; set; } = default!;

    [Inject]
    public ICategoryRepository CategoryRepositoryAsync { get; set; } = default!;

    public bool IsShow { get; set; }

    public int CategoryId { get; set; }

    public List<Category> Categories { get; set; } = new();

    public void Show()
    {
        IsShow = true;
    }

    public void Close()
    {
        IsShow = false;
    }

    protected async Task btnSaveOrUpdate_Click()
    {
        Model.CategoryId = CategoryId;

        if (Model.ProductId == 0)
        {
            await ProductRepositoryAsync.AddAsync(Model);
        }
        else
        {
            await ProductRepositoryAsync.EditAsync(Model);
        }

        IsShow = false;

        await SaveOrUpdated.InvokeAsync();
    }

    protected override void OnParametersSet()
    {
        CategoryId = Model.CategoryId;
    }

    protected override async Task OnInitializedAsync()
    {
        Categories = await CategoryRepositoryAsync.GetAllAsync();
    }
}
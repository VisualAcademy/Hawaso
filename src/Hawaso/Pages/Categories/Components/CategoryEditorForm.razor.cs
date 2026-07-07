using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Hawaso.Pages.Categories.Components;

public partial class CategoryEditorForm
{
    [Parameter]
    public RenderFragment? EditorFormTitle { get; set; }

    [Parameter]
    public Category Model { get; set; } = new Category();

    [Parameter]
    public EventCallback SaveOrUpdated { get; set; }

    [Parameter]
    public EventCallback ChangeCallback { get; set; }

    [Inject]
    public ICategoryRepository CategoryRepositoryAsync { get; set; } = default!;

    public bool IsShow { get; set; }

    public void Show()
    {
        IsShow = true;
    }

    public void Close()
    {
        IsShow = false;
    }

    protected async Task btnSaveOrUpdate_ClickAsync()
    {
        if (Model.CategoryId == 0)
        {
            await CategoryRepositoryAsync.AddAsync(Model);

            if (SaveOrUpdated.HasDelegate)
            {
                await SaveOrUpdated.InvokeAsync();
            }
        }
        else
        {
            await CategoryRepositoryAsync.EditAsync(Model);

            if (ChangeCallback.HasDelegate)
            {
                await ChangeCallback.InvokeAsync();
            }
        }

        IsShow = false;
    }
}
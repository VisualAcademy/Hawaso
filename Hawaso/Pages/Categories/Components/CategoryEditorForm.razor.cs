using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System;

namespace Hawaso.Pages.Categories.Components
{
    public partial class CategoryEditorForm
    {
        [Parameter]
        public RenderFragment EditorFormTitle { get; set; }

        [Parameter]
        public Category Model { get; set; }

        [Parameter]
        public Action SaveOrUpdated { get; set; } // EventCallback<bool> 

        [Parameter]
        public EventCallback<bool> ChangeCallback { get; set; }

        [Inject]
        public ICategoryRepository CategoryRepositoryAsync { get; set; }

        public bool IsShow { get; set; }

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
            if (Model.CategoryId == 0)
            {
                await CategoryRepositoryAsync.AddAsync(Model);
                SaveOrUpdated?.Invoke(); 
            }
            else
            {
                await CategoryRepositoryAsync.EditAsync(Model); 
                await ChangeCallback.InvokeAsync(true);
            }
            IsShow = false;
        }
    }
}

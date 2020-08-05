using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System;

namespace Hawaso.Pages.Customers.Components
{
    public partial class CustomerEditorForm
    {
        [Parameter]
        public RenderFragment EditorFormTitle { get; set; }

        [Parameter]
        public Customer Model { get; set; }

        [Parameter]
        public Action SaveOrUpdated { get; set; } // EventCallback<bool> 

        [Parameter]
        public EventCallback<bool> ChangeCallback { get; set; }

        [Inject]
        public ICustomerRepository CustomerRepositoryAsync { get; set; }

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
            if (Model.CustomerId == 0)
            {
                await CustomerRepositoryAsync.AddAsync(Model);
                SaveOrUpdated?.Invoke(); 
            }
            else
            {
                await CustomerRepositoryAsync.EditAsync(Model); 
                await ChangeCallback.InvokeAsync(true);
            }
            IsShow = false;
        }
    }
}

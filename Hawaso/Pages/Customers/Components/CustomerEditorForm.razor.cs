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

        public Customer ModelEdit { get; set; }

        // 넘어온 Model 값을 수정 전용 ModelEdit에 담기 
        protected override void OnParametersSet()
        {
            ModelEdit = new Customer(); 
            ModelEdit.CustomerId = Model.CustomerId;
            ModelEdit.CustomerName = Model.CustomerName;
            ModelEdit.EmailAddress = Model.EmailAddress; 
        }

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
            // 변경 내용 저장
            Model.CustomerName = ModelEdit.CustomerName;
            Model.EmailAddress = ModelEdit.EmailAddress;

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

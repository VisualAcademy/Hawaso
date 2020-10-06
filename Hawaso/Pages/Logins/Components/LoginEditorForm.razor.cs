using Hawaso.Models;
using Microsoft.AspNetCore.Components;
using System;

namespace Hawaso.Pages.Logins.Components
{
    public partial class LoginEditorForm
    {
        [Parameter]
        public RenderFragment EditorFormTitle { get; set; }

        [Parameter]
        public Login Model { get; set; }

        [Parameter]
        public Action SaveOrUpdated { get; set; } // EventCallback<bool> 

        [Parameter]
        public EventCallback<bool> ChangeCallback { get; set; }

        [Inject]
        public ILoginRepositoryAsync LoginRepositoryAsync { get; set; }

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
            if (Model.LoginId == 0)
            {
                await LoginRepositoryAsync.AddAsync(Model);
                SaveOrUpdated?.Invoke();
            }
            else
            {
                await LoginRepositoryAsync.EditAsync(Model);
                await ChangeCallback.InvokeAsync(true);
            }
            IsShow = false;
        }
    }
}

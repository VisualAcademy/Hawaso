using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Hawaso.Pages.Logins.Components
{
    public partial class LoginDeleteDialog
    {
        public bool IsShow { get; set; }

        public void Show()
        {
            IsShow = true;
        }

        public void Close()
        {
            IsShow = false;
        }

        [Parameter]
        public EventCallback<MouseEventArgs> OnClick { get; set; }
    }
}

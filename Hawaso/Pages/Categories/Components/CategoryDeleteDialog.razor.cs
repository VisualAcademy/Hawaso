using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Hawaso.Pages.Categories.Components
{
    public partial class CategoryDeleteDialog
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

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Hawaso.Pages.Products.Components
{
    public partial class ProductDeleteDialog
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

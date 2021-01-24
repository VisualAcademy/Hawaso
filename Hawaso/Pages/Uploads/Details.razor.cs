using Microsoft.AspNetCore.Components;
using UploadApp.Models;
using System.Threading.Tasks;

namespace Hawaso.Pages.Uploads
{
    public partial class Details
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public IUploadRepository UploadRepositoryAsyncReference { get; set; }

        protected Upload model = new Upload();

        protected string content = "";

        protected override async Task OnInitializedAsync()
        {
            model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);
            content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
        }
    }
}

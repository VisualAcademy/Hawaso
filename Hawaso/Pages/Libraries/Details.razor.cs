using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Zero.Models;

namespace Hawaso.Pages.Libraries
{
    public partial class Details
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public ILibraryRepository UploadRepositoryAsyncReference { get; set; }

        protected LibraryModel model = new LibraryModel();

        protected string content = "";

        protected override async Task OnInitializedAsync()
        {
            model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);
            content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
        }
    }
}

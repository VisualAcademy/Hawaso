using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Zero.Models;

namespace Hawaso.Pages.BriefingLogs
{
    public partial class Details
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; }

        protected BriefingLog model = new BriefingLog();

        protected string content = "";

        protected override async Task OnInitializedAsync()
        {
            model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);
            content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
        }
    }
}

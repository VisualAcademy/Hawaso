using Microsoft.AspNetCore.Components;
using NoticeApp.Models;
using System.Threading.Tasks;

namespace Hawaso.Pages.Notices
{
    public partial class Details
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public INoticeRepository NoticeRepositoryAsyncReference { get; set; }

        protected Notice model = new Notice();

        protected string content = "";

        protected override async Task OnInitializedAsync()
        {
            model = await NoticeRepositoryAsyncReference.GetByIdAsync(Id);
            //content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
            // HTML 태그 실행을 위한 인코딩없이 바로 전달
            content = model.Content;
        }
    }
}

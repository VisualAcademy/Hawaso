using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NoticeApp.Models;
using System.Threading.Tasks;

namespace Hawaso.Pages.Notices
{
    public partial class Delete
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public INoticeRepository NoticeRepositoryAsyncReference { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public NavigationManager NavigationManagerReference { get; set; }

        protected Notice model = new Notice();

        protected string content = "";

        protected override async Task OnInitializedAsync()
        {
            model = await NoticeRepositoryAsyncReference.GetByIdAsync(Id);
            content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
        }

        protected async void DeleteClick()
        {
            bool isDelete = await JSRuntime.InvokeAsync<bool>("confirm", $"{Id}번 글을 정말로 삭제하시겠습니까?");

            if (isDelete)
            {
                await NoticeRepositoryAsyncReference.DeleteAsync(Id); // 삭제
                NavigationManagerReference.NavigateTo("/Notices"); // 리스트 페이지로 이동
            }
            else
            {
                await JSRuntime.InvokeAsync<object>("alert", "취소되었습니다.");
            }
        }
    }
}

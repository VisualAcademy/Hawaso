using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UploadApp.Models;
using System.Threading.Tasks;
using ReplyApp.Managers;

namespace Hawaso.Pages.Uploads
{
    public partial class Delete
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public IUploadRepository UploadRepositoryAsyncReference { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public NavigationManager NavigationManagerReference { get; set; }

        protected Upload model = new Upload();

        protected string content = "";

        protected override async Task OnInitializedAsync()
        {
            model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);
            content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
        }

        protected async void DeleteClick()
        {
            bool isDelete = await JSRuntime.InvokeAsync<bool>("confirm", $"{Id}번 글을 정말로 삭제하시겠습니까?");

            if (isDelete)
            {
                if (!string.IsNullOrEmpty(model?.FileName))
                {
                    // 첨부 파일 삭제 
                    await FileStorageManager.DeleteAsync(model.FileName, "");
                }

                await UploadRepositoryAsyncReference.DeleteAsync(Id); // 삭제
                NavigationManagerReference.NavigateTo("/Uploads"); // 리스트 페이지로 이동
            }
            else
            {
                await JSRuntime.InvokeAsync<object>("alert", "취소되었습니다.");
            }
        }

        [Inject]
        public IFileStorageManager FileStorageManager { get; set; }
    }
}

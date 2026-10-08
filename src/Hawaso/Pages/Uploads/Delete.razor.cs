using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Uploads
{
    public partial class Delete
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public IUploadRepository UploadRepositoryAsyncReference { get; set; } = default!;

        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManagerReference { get; set; } = default!;

        [Inject]
        public IFileStorageManager FileStorageManager { get; set; } = default!;

        protected Upload model = new Upload();

        protected string content = "";

        protected override async Task OnInitializedAsync()
        {
            model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);
            content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
        }

        protected async Task DeleteClick()
        {
            bool isDelete = await JSRuntime.InvokeAsync<bool>(
                "confirm",
                $"{Id}번 글을 정말로 삭제하시겠습니까?");

            if (isDelete)
            {
                if (!string.IsNullOrEmpty(model?.FileName))
                {
                    // 첨부 파일 삭제
                    await FileStorageManager.DeleteAsync(model.FileName, "");
                }

                // 데이터 삭제
                await UploadRepositoryAsyncReference.DeleteAsync(Id);

                // 리스트 페이지로 이동
                NavigationManagerReference.NavigateTo("/Uploads");
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", "취소되었습니다.");
            }
        }
    }
}
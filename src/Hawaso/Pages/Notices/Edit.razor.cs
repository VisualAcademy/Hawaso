using Microsoft.AspNetCore.Components;
using NoticeApp.Models;
using System.Threading.Tasks;

namespace Hawaso.Pages.Notices
{
    public partial class Edit
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        public INoticeRepository NoticeRepositoryAsyncReference { get; set; }

        [Inject]
        public NavigationManager NavigationManagerReference { get; set; }

        protected Notice model = new Notice();

        public string ParentId { get; set; }

        protected int[] parentIds = { 1, 2, 3 };

        protected string content = "";

        protected override async Task OnInitializedAsync()
        {
            model = await NoticeRepositoryAsyncReference.GetByIdAsync(Id);
            content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
            ParentId = model.ParentId.ToString(); 
        }

        protected async void FormSubmit()
        {
            int.TryParse(ParentId, out int parentId);
            model.ParentId = parentId;
            await NoticeRepositoryAsyncReference.EditAsync(model);
            NavigationManagerReference.NavigateTo("/Notices");
        }
    }
}

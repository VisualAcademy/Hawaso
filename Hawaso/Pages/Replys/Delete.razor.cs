using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ReplyApp.Models;
using System.Threading.Tasks;
using ReplyApp.Managers;

namespace Hawaso.Pages.Replys
{
    public partial class Delete
    {
        #region Parameters
        [Parameter]
        public int Id { get; set; }
        #endregion

        #region Injectors
        [Inject]
        public IJSRuntime JSRuntimeInjector { get; set; }

        [Inject]
        public NavigationManager NavigationManagerInjector { get; set; } 

        [Inject]
        public IReplyRepository RepositoryReference { get; set; }

        [Inject]
        public IFileStorageManager FileStorageManagerReference { get; set; }
        #endregion

        #region Properties
        public Reply Model { get; set; } = new Reply();

        public string Content { get; set; } = "";
        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// 페이지 초기화 이벤트 처리기
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            Model = await RepositoryReference.GetByIdAsync(Id);
            Content = Dul.HtmlUtility.EncodeWithTabAndSpace(Model.Content);
        } 
        #endregion

        #region Event Handlers

        /// <summary>
        /// 삭제 버튼 클릭 이벤트 처리기
        /// </summary>
        protected async void DeleteClick()
        {
            bool isDelete = await JSRuntimeInjector.InvokeAsync<bool>("confirm", $"{Id}번 글을 정말로 삭제하시겠습니까?");

            if (isDelete)
            {
                if (!string.IsNullOrEmpty(Model?.FileName))
                {
                    // 첨부 파일 삭제 
                    await FileStorageManagerReference.DeleteAsync(Model.FileName, "");
                }

                await RepositoryReference.DeleteAsync(Id); // 삭제
                NavigationManagerInjector.NavigateTo("/Replys"); // 리스트 페이지로 이동
            }
            else
            {
                await JSRuntimeInjector.InvokeAsync<object>("alert", "취소되었습니다.");
            }
        } 
        #endregion
    }
}

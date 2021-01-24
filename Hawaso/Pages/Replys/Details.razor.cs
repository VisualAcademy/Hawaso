using Microsoft.AspNetCore.Components;
using ReplyApp.Models;
using System.Threading.Tasks;

namespace Hawaso.Pages.Replys
{
    public partial class Details
    {
        #region Parameters
        [Parameter]
        public int Id { get; set; }
        #endregion

        #region Injectors
        [Inject]
        public IReplyRepository RepositoryReference { get; set; }
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
    }
}

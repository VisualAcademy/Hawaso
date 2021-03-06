using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Hawaso.Pages.Libraries.Components
{
    public partial class DeleteDialog
    {
        /// <summary>
        /// 모달 다이얼로그를 표시할건지 여부 
        /// </summary>
        public bool IsShow { get; set; } = false;

        /// <summary>
        /// 폼 보이기 
        /// </summary>
        public void Show() => IsShow = true;

        /// <summary>
        /// 폼 닫기
        /// </summary>
        public void Hide()
        {
            IsShow = false;
        }

        /// <summary>
        /// 부모에서 OnClick 속성에 지정한 이벤트 처리기 실행
        /// </summary>
        [Parameter]
        public EventCallback<MouseEventArgs> OnClick { get; set; }
    }
}

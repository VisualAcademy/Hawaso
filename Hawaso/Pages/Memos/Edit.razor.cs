using BlazorInputFile;
using Hawaso.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Memos
{
    public partial class Edit
    {
        #region Fields
        /// <summary>
        /// 첨부 파일 리스트 보관
        /// </summary>
        private IFileListEntry[] selectedFiles;

        /// <summary>
        /// 부모(카테고리) 리스트가 저장될 임시 변수
        /// </summary>
        protected int[] parentIds = { 1, 2, 3 };
        #endregion

        #region Parameters
        [Parameter]
        public int Id { get; set; }
        #endregion

        #region Injectors
        [Inject]
        public IMemoRepository RepositoryReference { get; set; }

        [Inject]
        public NavigationManager NavigationManagerInjector { get; set; }

        [Inject]
        public IMemoFileStorageManager FileStorageManagerInjector { get; set; }
        #endregion

        #region Properties
        //protected Memo Model = new Memo();
        public Memo Model { get; set; } = new Memo();

        public string ParentId { get; set; } = "";

        //protected string Content = "";
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
            ParentId = Model.ParentId.ToString();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 수정 버튼 이벤트 처리기
        /// </summary>
        protected async void FormSubmit()
        {
            int.TryParse(ParentId, out int parentId);
            Model.ParentId = parentId;

            #region 파일 업로드 관련 추가 코드 영역
            if (selectedFiles != null && selectedFiles.Length > 0)
            {
                // 파일 업로드
                var file = selectedFiles.FirstOrDefault();
                var fileName = "";
                int fileSize = 0;
                if (file != null)
                {
                    fileName = file.Name;
                    fileSize = Convert.ToInt32(file.Size);

                    // 첨부 파일 삭제 
                    await FileStorageManagerInjector.DeleteAsync(Model.FileName, "Memos");

                    // 다시 업로드
                    fileName = await FileStorageManagerInjector.UploadAsync(file.Data, file.Name, "", true);

                    Model.FileName = fileName;
                    Model.FileSize = fileSize;
                }
            }
            #endregion

            await RepositoryReference.EditAsync(Model);
            NavigationManagerInjector.NavigateTo("/Memos");
        }

        /// <summary>
        /// 파일 선택 이벤트 처리기
        /// </summary>
        protected void HandleSelection(IFileListEntry[] files)
        {
            this.selectedFiles = files;
        }
        #endregion
    }
}

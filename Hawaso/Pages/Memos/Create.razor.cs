using BlazorInputFile;
using Hawaso.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Memos
{
    public partial class Create
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
        public int Id { get; set; } = 0;
        #endregion

        #region Injectors
        // Reference 접미사 사용해 봄
        [Inject]
        public IMemoRepository RepositoryReference { get; set; }

        // Injector 접미사 사용해 봄 
        [Inject]
        public NavigationManager NavigationManagerInjector { get; set; }

        [Inject]
        public IMemoFileStorageManager FileStorageManagerInjector { get; set; }
        #endregion

        #region Properties
        private Memo Model { get; set; } = new Memo();

        public string Name { get; set; }
        public string Title { get; set; }

        public string Content { get; set; } = "";

        public string ParentId { get; set; } = "";

        // 부모 글의 답변형 게시판 계층 정보를 임시 보관
        public int ParentRef { get; set; } = 0;
        public int ParentStep { get; set; } = 0;
        public int ParentRefOrder { get; set; } = 0;
        #endregion

        #region Lifecycle Methods
        /// <summary>
        /// 페이지 초기화 이벤트 처리기
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            // 답변 글쓰기 페이지라면, 기존 데이터 읽어오기 
            if (Id != 0)
            {
                // 기존 글의 데이터를 읽어오기 
                var parent = await RepositoryReference.GetByIdAsync(Id);

                //Model.Id = 0; // 답변 페이지는 새로운 글로 초기화 
                Model.Name = "";
                Model.Title = "Re: " + parent.Title;
                Model.Content = "\r\n====\r\n" + parent.Content;

                ParentRef = (int)parent.Ref;
                ParentStep = (int)parent.Step;
                ParentRefOrder = (int)parent.RefOrder;
            }
        }
        #endregion

        #region Event Handlers

        /// <summary>
        /// 저장하기 버튼 클릭 이벤트 처리기
        /// </summary>
        protected async void FormSubmit()
        {
            int.TryParse(ParentId, out int parentId); // 드롭다운 선택 값을 정수형으로 변환
            Model.ParentId = parentId; // 선택한 ParentId 값 가져오기 

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

                    fileName = await FileStorageManagerInjector.UploadAsync(file.Data, file.Name, "Memos", true);

                    Model.FileName = fileName;
                    Model.FileSize = fileSize;
                }
            }
            #endregion

            var m = new Memo();

            m.Name = Model.Name;
            m.Title = Model.Title;
            m.Content = Model.Content;
            m.Password = Model.Password;
            m.Email = Model.Email;
            m.FileName = Model.FileName;
            m.FileSize = Model.FileSize;

            m.PostDate = DateTime.Now;
            m.ParentNum = 0;
            m.AnswerNum = 0;
            m.CommentCount = 0;
            m.Created = DateTime.UtcNow;
            m.CreatedBy = "";
            m.Category = "Free";
            m.AnswerNum = 0;
            m.CommentCount = 0;
            m.Encoding = "Text";
            m.Email = "";
            m.IsPinned = false;
            m.Modified = DateTime.Now;
            m.ModifyIp = "";
            m.PostIp = "127.0.0.1";
            m.Step = 0;
            m.RefOrder = 0;


            if (Id != 0)
            {
                // Memo: 답변 글이라면,
                await RepositoryReference.AddAsync(m, Id);
            }
            else
            {
                // Create: 일반 작성 글이라면,
                await RepositoryReference.AddAsync(m);
            }

            // Manage 컴포넌트로 이동 
            NavigationManagerInjector.NavigateTo("/Memos");
        }

        /// <summary>
        /// 파일 첨부 이벤트 처리기 
        /// </summary>
        protected void HandleSelection(IFileListEntry[] files)
        {
            this.selectedFiles = files;
        }
        #endregion
    }
}

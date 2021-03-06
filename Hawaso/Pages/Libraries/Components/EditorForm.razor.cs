using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using Zero.Models;

namespace Hawaso.Pages.Libraries.Components
{
    public partial class EditorForm
    {
        /// <summary>
        /// 모달 다이얼로그를 표시할건지 여부 
        /// </summary>
        public bool IsShow { get; set; } = false;

        private string parentId = "0";

        [Parameter]
        public string ParentKey { get; set; } = "";

        protected int[] parentIds = { 1, 2, 3 };

        /// <summary>
        /// 폼 보이기 
        /// </summary>
        public void Show()
        {
            IsShow = true;
        }

        /// <summary>
        /// 폼 닫기
        /// </summary>
        public void Hide()
        {
            IsShow = false;
        }

        /// <summary>
        /// 폼의 제목 영역
        /// </summary>
        [Parameter]
        public RenderFragment EditorFormTitle { get; set; }

        /// <summary>
        /// 넘어온 모델 개체 
        /// </summary>
        [Parameter]
        public LibraryModel Model { get; set; }

        /// <summary>
        /// 부모 컴포넌트에게 생성(Create)이 완료되었다고 보고하는 목적으로 부모 컴포넌트에게 알림
        /// </summary>
        [Parameter]
        public Action CreateCallback { get; set; }

        /// <summary>
        /// 부모 컴포넌트에게 수정(Edit)이 완료되었다고 보고하는 목적으로 부모 컴포넌트에게 알림
        /// </summary>
        [Parameter]
        public EventCallback<bool> EditCallback { get; set; }

        /// <summary>
        /// 리포지토리 클래스에 대한 참조 
        /// </summary>
        [Inject]
        public ILibraryRepository UploadRepositoryAsyncReference { get; set; }

        protected override void OnParametersSet()
        {
            parentId = Model.ParentId.ToString();
            if (parentId == "0")
            {
                parentId = "";
            }
        }

        protected async void CreateOrEditClick()
        {
            #region 파일 업로드 관련 추가 코드 영역
            if (selectedFiles != null && selectedFiles.Length > 0)
            {
                // 파일 업로드
                var file = selectedFiles.FirstOrDefault();
                var fileName = "";
                int fileSize = 0;
                if (file != null)
                {
                    //file.Name = $"{DateTime.Now.ToString("yyyyMMddhhmmss")}{file.Name}";
                    fileName = file.Name;
                    fileSize = Convert.ToInt32(file.Size);

                    //[A] byte[] 형태
                    //var ms = new MemoryStream();
                    //await file.Data.CopyToAsync(ms);
                    //await FileStorageManager.UploadAsync(ms.ToArray(), file.Name, "", true);
                    //[B] Stream 형태
                    //string folderPath = Path.Combine(WebHostEnvironment.WebRootPath, "files");
                    await FileStorageManager.UploadAsync(file.Data, file.Name, "", true);

                    Model.FileName = fileName;
                    Model.FileSize = fileSize;
                }
            }
            #endregion

            if (!int.TryParse(parentId, out int newParentId))
            {
                newParentId = 0;
            }
            Model.ParentId = newParentId;
            Model.ParentKey = Model.ParentKey;

            if (Model.Id == 0)
            {
                // Create
                await UploadRepositoryAsyncReference.AddAsync(Model);
                CreateCallback?.Invoke();
            }
            else
            {
                // Edit
                await UploadRepositoryAsyncReference.EditAsync(Model);
                await EditCallback.InvokeAsync(true);
            }
            //IsShow = false; // this.Hide()
        }

        private IFileListEntry[] selectedFiles;
        protected void HandleSelection(IFileListEntry[] files)
        {
            this.selectedFiles = files;
        }

        [Inject]
        public ILibraryFileStorageManager FileStorageManager { get; set; }
    }
}

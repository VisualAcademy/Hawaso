using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using UploadApp.Models;
using ReplyApp.Managers;

namespace Hawaso.Pages.Uploads
{
    public partial class Create
    {
        [Inject]
        public IUploadRepository UploadRepositoryAsyncReference { get; set; }

        [Inject]
        public NavigationManager NavigationManagerReference { get; set; }

        protected Upload model = new Upload();

        public string ParentId { get; set; }

        protected int[] parentIds = { 1, 2, 3 };

        protected async void FormSubmit()
        {
            int.TryParse(ParentId, out int parentId);
            model.ParentId = parentId;

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

                    fileName = await FileStorageManager.UploadAsync(file.Data, file.Name, "", true);

                    model.FileName = fileName;
                    model.FileSize = fileSize;
                } 
            }
            #endregion

            await UploadRepositoryAsyncReference.AddAsync(model);
            NavigationManagerReference.NavigateTo("/Uploads");
        }

        [Inject]
        public IFileStorageManager FileStorageManager { get; set; }
        private IFileListEntry[] selectedFiles;
        protected void HandleSelection(IFileListEntry[] files)
        {
            this.selectedFiles = files;
        }
    }
}

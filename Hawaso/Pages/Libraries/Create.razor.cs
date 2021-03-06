using BlazorInputFile;
using Hawaso.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using Zero.Models;

namespace Hawaso.Pages.Libraries
{
    public partial class Create
    {
        [Inject]
        public ILibraryRepository UploadRepositoryAsyncReference { get; set; }

        [Inject]
        public NavigationManager NavigationManagerReference { get; set; }

        protected LibraryModel model = new LibraryModel();

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

                    fileName = await FileStorageManager.UploadAsync(file.Data, file.Name, "Libraries", true);

                    model.FileName = fileName;
                    model.FileSize = fileSize;
                }
            }
            #endregion

            await UploadRepositoryAsyncReference.AddAsync(model);
            NavigationManagerReference.NavigateTo("/Libraries");
        }

        [Inject]
        public ILibraryFileStorageManager FileStorageManager { get; set; }
        private IFileListEntry[] selectedFiles;
        protected void HandleSelection(IFileListEntry[] files)
        {
            this.selectedFiles = files;
        }

        protected override async Task OnInitializedAsync()
        {
            await GetUserIdAndUserName();
            model.Name = UserName; 
        }

        #region Get UserId and UserName
        [Parameter]
        public string UserId { get; set; } = "";

        [Parameter]
        public string UserName { get; set; } = "";

        [Inject] public UserManager<ApplicationUser> UserManagerRef { get; set; }

        // [CascadingParameter] Task<AuthenticationState> authenticationStateTask { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; }

        private async Task GetUserIdAndUserName()
        {
            var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity.IsAuthenticated)
            {
                var currentUser = await UserManagerRef.GetUserAsync(user);
                UserId = currentUser.Id;
                UserName = user.Identity.Name;
            }
            else
            {
                UserId = "";
                UserName = "Anonymous";
            }
        }
        #endregion
    }
}

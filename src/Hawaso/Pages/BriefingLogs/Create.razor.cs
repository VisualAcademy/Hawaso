﻿using BlazorInputFile;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Zero.Models;

namespace Hawaso.Pages.BriefingLogs;

public partial class Create
{
    #region Injectors
    [Inject]
    public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; }

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; }
    #endregion

    #region Properties
    public BriefingLog Model { get; set; }

    public string ParentId { get; set; }
    #endregion

    #region Fields
    protected int[] parentIds = { 1, 2, 3 };
    #endregion

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

                fileName = await FileStorageManager.UploadAsync(file.Data, file.Name, "BriefingLogs", true);

                Model.FileName = fileName;
                Model.FileSize = fileSize;
            }
        }
        #endregion

        await UploadRepositoryAsyncReference.AddAsync(Model);
        NavigationManagerReference.NavigateTo("/BriefingLogs");
    }

    [Inject]
    public IBriefingLogFileStorageManager FileStorageManager { get; set; }
    private IFileListEntry[] selectedFiles;
    protected void HandleSelection(IFileListEntry[] files) => this.selectedFiles = files;

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        if (UserId == "" && UserName == "")
        {
            await GetUserIdAndUserName();
        }

        Model.Name = UserName;
        Model.DateTimeStarted = DateTime.Today;
    }
    #endregion

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

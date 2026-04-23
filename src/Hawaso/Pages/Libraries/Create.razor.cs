using BlazorInputFile;
using Hawaso.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using VisualAcademy.Models.Libraries;

namespace Hawaso.Pages.Libraries;

public partial class Create
{
    [Inject]
    public ILibraryRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; } = default!;

    [Inject]
    public ILibraryFileStorageManager FileStorageManager { get; set; } = default!;

    [Inject]
    public UserManager<ApplicationUser> UserManagerRef { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; } = default!;

    protected LibraryModel model = new();

    public string ParentId { get; set; } = "";

    protected int[] parentIds = { 1, 2, 3 };

    private IFileListEntry[]? selectedFiles;

    [Parameter]
    public string UserId { get; set; } = "";

    [Parameter]
    public string UserName { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(UserId) && string.IsNullOrEmpty(UserName))
        {
            await GetUserIdAndUserName();
        }

        model.Name = UserName;
    }

    protected async Task FormSubmit()
    {
        int.TryParse(ParentId, out int parentId);
        model.ParentId = parentId;

        #region 파일 업로드 관련 추가 코드 영역
        if (selectedFiles is { Length: > 0 })
        {
            var file = selectedFiles.FirstOrDefault();

            if (file is not null)
            {
                var fileSize = Convert.ToInt32(file.Size);
                var uploadedFileName = await FileStorageManager.UploadAsync(
                    file.Data,
                    file.Name,
                    "Libraries",
                    true);

                model.FileName = uploadedFileName;
                model.FileSize = fileSize;
            }
        }
        #endregion

        await UploadRepositoryAsyncReference.AddAsync(model);
        NavigationManagerReference.NavigateTo("/Libraries");
    }

    protected void HandleSelection(IFileListEntry[] files) => selectedFiles = files;

    #region Get UserId and UserName
    private async Task GetUserIdAndUserName()
    {
        var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
        var user = authState.User;
        var identity = user.Identity;

        if (identity?.IsAuthenticated == true)
        {
            var currentUser = await UserManagerRef.GetUserAsync(user);

            UserId = currentUser?.Id ?? "";
            UserName = identity.Name ?? currentUser?.UserName ?? "Anonymous";
        }
        else
        {
            UserId = "";
            UserName = "Anonymous";
        }
    }
    #endregion
}
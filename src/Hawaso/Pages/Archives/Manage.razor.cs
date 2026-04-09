using BlazorUtils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using VisualAcademy.Models.Archives;

namespace Hawaso.Pages.Archives;

public partial class Manage
{
    #region Parameters
    [Parameter] public int ParentId { get; set; }
    [Parameter] public string ParentKey { get; set; } = string.Empty;
    [Parameter] public string UserId { get; set; } = string.Empty;
    [Parameter] public string UserName { get; set; } = string.Empty;
    #endregion

    #region Injectors
    [Inject] public NavigationManager Nav { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntimeInjector { get; set; } = default!;
    [Inject] public IArchiveRepository RepositoryReference { get; set; } = default!;
    [Inject] public IArchiveFileStorageManager FileStorageManagerReference { get; set; } = default!;
    [Inject] public UserManager<ApplicationUser> UserManagerRef { get; set; } = default!;
    [Inject] public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; } = default!;
    #endregion

    #region Properties
    /// <summary>
    /// 글쓰기 또는 수정하기 폼의 제목(태그 포함 가능)
    /// </summary>
    public string EditorFormTitle { get; set; } = "CREATE";
    #endregion

    #region References
    public Components.ModalForm? EditorFormReference { get; set; }
    public Components.DeleteDialog? DeleteDialogReference { get; set; }
    #endregion

    #region Fields
    protected List<Archive> models = new();
    protected Archive model = new();

    protected DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    private string searchQuery = string.Empty;
    private string sortOrder = string.Empty;
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(UserId) && string.IsNullOrEmpty(UserName))
        {
            await GetUserIdAndUserName();
        }

        await DisplayData();
    }
    #endregion

    #region Data
    private async Task DisplayData()
    {
        if (!string.IsNullOrEmpty(ParentKey))
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<string>(
                pager.PageIndex,
                pager.PageSize,
                searchField: "",
                searchQuery,
                sortOrder,
                ParentKey);

            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else if (ParentId != 0)
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(
                pager.PageIndex,
                pager.PageSize,
                searchField: "",
                searchQuery,
                sortOrder,
                ParentId);

            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(
                pager.PageIndex,
                pager.PageSize,
                searchField: "",
                searchQuery,
                sortOrder,
                parentIdentifier: 0);

            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }

        StateHasChanged();
    }
    #endregion

    #region Paging / Navigation
    protected void NameClick(long id) => Nav.NavigateTo($"/Archives/Details/{id}");

    protected async Task PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;
        await DisplayData();
    }
    #endregion

    #region Event Handlers
    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";
        model = new Archive
        {
            ParentId = ParentId,
            ParentKey = ParentKey,
            Name = UserName
        };

        EditorFormReference?.Show();
    }

    protected void EditBy(Archive selected)
    {
        model = selected ?? new Archive();
        model.ParentId = ParentId;
        model.ParentKey = ParentKey;
        EditorFormTitle = "EDIT";

        EditorFormReference?.Show();
    }

    protected void DeleteBy(Archive selected)
    {
        model = selected ?? new Archive();
        DeleteDialogReference?.Show();
    }

    protected async Task DownloadBy(Archive selected)
    {
        if (string.IsNullOrWhiteSpace(selected.FileName))
        {
            return;
        }

        var fileBytes = await FileStorageManagerReference.DownloadAsync(selected.FileName, "Archives");
        if (fileBytes is not { Length: > 0 })
        {
            return;
        }

        selected.DownCount = (selected.DownCount ?? 0) + 1;
        await RepositoryReference.EditAsync(selected);

        await FileUtil.SaveAs(JSRuntimeInjector, selected.FileName, fileBytes);
    }

    protected async void CreateOrEdit()
    {
        EditorFormReference?.Hide();
        model = new Archive();
        await DisplayData();
    }

    protected async Task DeleteClick()
    {
        if (!string.IsNullOrWhiteSpace(model.FileName))
        {
            await FileStorageManagerReference.DeleteAsync(model.FileName, "Archives");
        }

        await RepositoryReference.DeleteAsync(model.Id);
        DeleteDialogReference?.Hide();

        model = new Archive();
        await DisplayData();
    }
    #endregion

    #region Toggle with Inline Dialog
    public bool IsInlineDialogShow { get; set; }

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        model = new Archive();
    }

    protected async Task ToggleClick()
    {
        if (model is null)
        {
            return;
        }

        model.IsPinned = model.IsPinned != true;

        await RepositoryReference.UpdateAsync(model);

        IsInlineDialogShow = false;
        model = new Archive();
        await DisplayData();
    }

    protected void ToggleBy(Archive selected)
    {
        model = selected ?? new Archive();
        IsInlineDialogShow = true;
    }
    #endregion

    #region Search
    protected async Task Search(string query)
    {
        pager.PageIndex = 0;
        searchQuery = query ?? string.Empty;
        await DisplayData();
    }
    #endregion

    #region Excel
    protected void DownloadExcelWithWebApi()
    {
        FileUtil.SaveAsExcel(JSRuntimeInjector, "/ArchiveDownload/ExcelDown");
        Nav.NavigateTo("/Archives");
    }

    protected void DownloadExcel() => DownloadExcelWithWebApi();
    #endregion

    #region Sorting
    protected async Task SortByName()
    {
        sortOrder = sortOrder switch
        {
            "" => "Name",
            "Name" => "NameDesc",
            _ => ""
        };

        await DisplayData();
    }

    protected async Task SortByTitle()
    {
        sortOrder = sortOrder switch
        {
            "" => "Title",
            "Title" => "TitleDesc",
            _ => ""
        };

        await DisplayData();
    }
    #endregion

    #region Get UserId and UserName
    private async Task GetUserIdAndUserName()
    {
        var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var currentUser = await UserManagerRef.GetUserAsync(user);
            UserId = currentUser?.Id ?? string.Empty;
            UserName = user.Identity.Name ?? currentUser?.UserName ?? "Anonymous";
        }
        else
        {
            UserId = string.Empty;
            UserName = "Anonymous";
        }
    }
    #endregion
}
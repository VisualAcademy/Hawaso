using BlazorUtils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using VisualAcademy.Models.Archives;

namespace Hawaso.Pages.Archives;

public partial class Manage
{
    #region Parameters
    [Parameter] public int ParentId { get; set; } = 0;
    [Parameter] public string ParentKey { get; set; } = "";
    #endregion

    #region Injectors
    [Inject] public NavigationManager Nav { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntimeInjector { get; set; } = default!;
    [Inject] public IArchiveRepository RepositoryReference { get; set; } = default!;
    [Inject] public IArchiveFileStorageManager FileStorageManagerReference { get; set; } = default!;
    #endregion

    #region Properties
    /// <summary>글쓰기 또는 수정하기 폼의 제목(태그 포함 가능)</summary>
    public string EditorFormTitle { get; set; } = "CREATE";
    #endregion

    // 모달 참조
    public Components.ModalForm? EditorFormReference { get; set; }
    public Components.DeleteDialog? DeleteDialogReference { get; set; }

    protected List<Archive> models = new();
    protected Archive model = new();

    protected DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

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

    private async Task DisplayData()
    {
        // 특정 부모 Details 페이지에서 리스트로 표현할 때 ParentKey/ParentId 사용
        if (!string.IsNullOrEmpty(ParentKey))
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<string>(pager.PageIndex, pager.PageSize, searchField: "", searchQuery, sortOrder, ParentKey);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else if (ParentId != 0)
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, searchField: "", searchQuery, sortOrder, ParentId);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }
        else
        {
            var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, searchField: "", searchQuery, sortOrder, parentIdentifier: 0);
            pager.RecordCount = articleSet.TotalCount;
            models = articleSet.Items.ToList();
        }

        StateHasChanged();
    }

    protected void NameClick(long id) => Nav.NavigateTo($"/Archives/Details/{id}");

    protected async Task PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;
        await DisplayData();
        StateHasChanged();
    }

    #region Event Handlers
    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";
        model = new Archive
        {
            ParentId = ParentId,
            ParentKey = ParentKey,
            Name = UserName // 로그인 사용자 이름 기본 제공
        };
        EditorFormReference?.Show();
    }

    protected void EditBy(Archive selected)
    {
        EditorFormTitle = "EDIT";
        model = selected;
        model.ParentId = ParentId;
        model.ParentKey = ParentKey;
        EditorFormReference?.Show();
    }

    protected void DeleteBy(Archive selected)
    {
        model = selected;
        DeleteDialogReference?.Show();
    }
    #endregion

    protected async Task DownloadBy(Archive selected)
    {
        if (!string.IsNullOrWhiteSpace(selected.FileName))
        {
            var fileBytes = await FileStorageManagerReference.DownloadAsync(selected.FileName, "Archives");
            if (fileBytes is { Length: > 0 })
            {
                // DownCount null-safe 증가
                selected.DownCount = (selected.DownCount ?? 0) + 1;
                await RepositoryReference.EditAsync(selected);

                await FileUtil.SaveAs(JSRuntimeInjector, selected.FileName, fileBytes);
            }
        }
    }

    protected async Task CreateOrEdit()
    {
        EditorFormReference?.Hide();
        model = new Archive();
        await DisplayData();
    }

    protected async Task DeleteClick()
    {
        if (!string.IsNullOrWhiteSpace(model?.FileName))
        {
            // 첨부 파일 삭제 
            await FileStorageManagerReference.DeleteAsync(model.FileName, "Archives");
        }

        await RepositoryReference.DeleteAsync(model.Id);
        DeleteDialogReference?.Hide();
        model = new Archive();
        await DisplayData();
    }

    #region Toggle with Inline Dialog
    public bool IsInlineDialogShow { get; set; } = false;

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        model = new Archive();
    }

    protected async Task ToggleClick()
    {
        model.IsPinned = model?.IsPinned == true ? false : true;

        await RepositoryReference.UpdateAsync(model);

        IsInlineDialogShow = false;
        model = new Archive();
        await DisplayData();
    }

    protected void ToggleBy(Archive selected)
    {
        model = selected;
        IsInlineDialogShow = true;
    }
    #endregion

    #region Search
    private string searchQuery = "";

    protected async Task Search(string query)
    {
        pager.PageIndex = 0;
        searchQuery = query;
        await DisplayData();
    }
    #endregion

    #region Excel (EPPlus 제거)
    // 서버의 OpenXML 기반 API를 호출하여 엑셀 다운로드
    protected void DownloadExcelWithWebApi()
    {
        FileUtil.SaveAsExcel(JSRuntimeInjector, "/ArchiveDownload/ExcelDown");
        Nav.NavigateTo("/Archives"); // 필요 시 강력 새로고침
    }

    // EPPlus 제거: 기존 메서드는 Web API 호출로 위임
    protected void DownloadExcel() => DownloadExcelWithWebApi();
    #endregion

    #region Sorting
    private string sortOrder = "";

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
    [Parameter] public string UserId { get; set; } = "";
    [Parameter] public string UserName { get; set; } = "";
    [Inject] public UserManager<ApplicationUser> UserManagerRef { get; set; } = default!;
    [Inject] public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; } = default!;

    private async Task GetUserIdAndUserName()
    {
        var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var currentUser = await UserManagerRef.GetUserAsync(user);
            UserId = currentUser?.Id ?? "";
            UserName = user.Identity?.Name ?? currentUser?.UserName ?? "Anonymous";
        }
        else
        {
            UserId = "";
            UserName = "Anonymous";
        }
    }
    #endregion
}

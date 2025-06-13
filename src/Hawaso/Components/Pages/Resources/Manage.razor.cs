using Azunt.Components.Dialogs;
using Azunt.Components.Paging;
using Azunt.ResourceManagement;
using Azunt.Web.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace Azunt.Web.Components.Pages.Resources;

public partial class Manage : ComponentBase
{
    public bool SimpleMode { get; set; } = false;

    #region Parameters
    [Parameter]
    public int ParentId { get; set; } = 0;

    [Parameter]
    public string ParentKey { get; set; } = "";
    #endregion

    #region Injectors
    [Inject]
    public NavigationManager NavigationManagerInjector { get; set; } = null!;

    [Inject]
    public IJSRuntime JSRuntimeInjector { get; set; } = null!;

    [Inject]
    public IResourceRepository RepositoryReference { get; set; } = null!;

    [Inject]
    public IConfiguration Configuration { get; set; } = null!;

    [Inject]
    public ResourceAppDbContextFactory DbContextFactory { get; set; } = null!;
    #endregion

    #region Properties
    public string EditorFormTitle { get; set; } = "CREATE";
    #endregion

    public Components.ModalForm EditorFormReference { get; set; } = null!;

    public DeleteDialog DeleteDialogReference { get; set; } = null!;

    protected List<Resource> models = new List<Resource>();

    protected Resource model = new Resource();

    protected PagerBase pager = new PagerBase()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 30,
        PagerButtonCount = 5
    };

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        if (UserId == "" && UserName == "")
        {
            await GetUserIdAndUserName();
        }

        await DisplayData();
    }
    #endregion

    private async Task DisplayData()
    {
        var appName = Configuration["AppName"] ?? "ReportWriter"; // appsettings.json에서 AppName 읽기

        var allModels = await RepositoryReference.GetAllByAppNameAsync(appName);

        // 검색 적용
        if (!string.IsNullOrEmpty(searchQuery))
        {
            allModels = allModels
                .Where(m => m.Alias != null && m.Alias.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        pager.RecordCount = allModels.Count;

        models = allModels
            .Skip(pager.PageIndex * pager.PageSize)
            .Take(pager.PageSize)
            .OrderBy(r => r.DisplayOrder)
            .ToList();

        StateHasChanged();
    }

    protected async void PageIndexChanged(int pageIndex)
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
        this.model = new Resource();
        EditorFormReference.Show();
    }

    protected void EditBy(Resource model)
    {
        EditorFormTitle = "EDIT";
        this.model = new Resource();
        this.model = model;
        EditorFormReference.Show();
    }

    protected void DeleteBy(Resource model)
    {
        this.model = model;
        DeleteDialogReference.Show();
    }
    #endregion

    protected async void CreateOrEdit()
    {
        EditorFormReference.Hide();

        await Task.Delay(50);

        this.model = new Resource();

        await DisplayData();
    }

    protected async void DeleteClick()
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        await RepositoryReference.DeleteAsync(this.model.Id, connectionString);
        DeleteDialogReference.Hide();
        this.model = new Resource();
        await DisplayData();
    }

    #region Inline Dialog for Toggle (optional, Resources에서는 보통 없음)
    public bool IsInlineDialogShow { get; set; } = false;

    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        this.model = new Resource();
    }

    protected async void ToggleClick()
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured properly.");
        }

        await using var context = DbContextFactory.CreateDbContext(connectionString);

        model.IsPublic = !model.IsPublic;

        context.Resources.Update(model);
        await context.SaveChangesAsync();

        IsInlineDialogShow = false;
        this.model = new Resource();

        await DisplayData();
    }

    protected void ToggleBy(Resource model)
    {
        this.model = model;
        IsInlineDialogShow = true;
    }
    #endregion

    #region Search
    private string searchQuery = "";

    protected async void Search(string query)
    {
        pager.PageIndex = 0;
        this.searchQuery = query;

        await DisplayData();
    }
    #endregion

    #region Sorting
    private string sortOrder = "";

    protected async void SortByAlias()
    {
        if (!sortOrder.Contains("Alias"))
        {
            sortOrder = "";
        }

        if (sortOrder == "")
        {
            sortOrder = "Alias";
        }
        else if (sortOrder == "Alias")
        {
            sortOrder = "AliasDesc";
        }
        else
        {
            sortOrder = "";
        }

        await DisplayData();
    }
    #endregion

    #region Get UserId and UserName
    [Parameter]
    public string UserId { get; set; } = "";

    [Parameter]
    public string UserName { get; set; } = "";

    [Inject] public UserManager<ApplicationUser> UserManagerRef { get; set; } = null!;

    [Inject] public AuthenticationStateProvider AuthenticationStateProviderRef { get; set; } = null!;

    private async Task GetUserIdAndUserName()
    {
        if (AuthenticationStateProviderRef == null || UserManagerRef == null)
        {
            UserId = "";
            UserName = "Anonymous";
            return;
        }

        var authState = await AuthenticationStateProviderRef.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var currentUser = await UserManagerRef.GetUserAsync(user);
            if (currentUser != null)
            {
                UserId = currentUser.Id;
                UserName = user.Identity?.Name ?? "Anonymous";
            }
            else
            {
                UserId = "";
                UserName = "Anonymous";
            }
        }
        else
        {
            UserId = "";
            UserName = "Anonymous";
        }
    }
    #endregion

    private async Task MoveUp(int id)
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        await RepositoryReference.MoveUpAsync(id, connectionString);
        await DisplayData();
    }

    private async Task MoveDown(int id)
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");
        await RepositoryReference.MoveDownAsync(id, connectionString);
        await DisplayData();
    }
}

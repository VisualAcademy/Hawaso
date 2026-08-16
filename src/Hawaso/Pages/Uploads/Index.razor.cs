using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.Uploads;

public partial class Index
{
    [Inject]
    public IUploadRepository UploadRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; } = default!;

    protected List<Upload>? models;

    protected DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    private string searchQuery = string.Empty;
    private string sortOrder = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await DisplayData();
    }

    private async Task DisplayData()
    {
        var articleSet = await UploadRepositoryAsyncReference.GetArticles<int>(
            pager.PageIndex,
            pager.PageSize,
            string.Empty,
            searchQuery,
            sortOrder,
            0);

        pager.RecordCount = articleSet.TotalCount;
        models = articleSet.Items.ToList();
    }

    protected void NameClick(int id)
    {
        NavigationManagerReference.NavigateTo($"/Uploads/Details/{id}");
    }

    protected async Task PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();
    }

    #region Search

    protected async Task Search(string query)
    {
        pager.PageIndex = 0;
        pager.PageNumber = 1;

        searchQuery = query;

        await DisplayData();
    }

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

        pager.PageIndex = 0;
        pager.PageNumber = 1;

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

        pager.PageIndex = 0;
        pager.PageNumber = 1;

        await DisplayData();
    }

    #endregion
}
using Microsoft.AspNetCore.Components;
using Zero.Models;

namespace Hawaso.Pages.BriefingLogs;

public partial class Index
{
    [Inject]
    public IBriefingLogRepository UploadRepositoryAsyncReference { get; set; }
        = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; }
        = default!;

    // 데이터가 로딩되기 전에는 null 상태를 사용합니다.
    protected List<BriefingLog>? models;

    protected readonly DulPager.DulPagerBase pager = new()
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
        var articleSet =
            await UploadRepositoryAsyncReference.GetArticles<int>(
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
        NavigationManagerReference.NavigateTo(
            $"/BriefingLogs/Details/{id}");
    }

    // DulPagerComponent 콜백 형식과의 호환성을 위해
    // async void를 유지합니다.
    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();

        StateHasChanged();
    }

    #region Search

    // SearchBox 콜백 형식과의 호환성을 위해
    // async void를 유지합니다.
    protected async void Search(string query)
    {
        pager.PageIndex = 0;
        pager.PageNumber = 1;

        searchQuery = query ?? string.Empty;

        await DisplayData();

        StateHasChanged();
    }

    #endregion

    #region Sorting

    // Razor onclick 이벤트에서 호출되므로 Task를 반환할 수 있습니다.
    protected async Task SortByName()
    {
        sortOrder = sortOrder switch
        {
            "" => "Name",
            "Name" => "NameDesc",
            _ => string.Empty
        };

        await DisplayData();
    }

    protected async Task SortByTitle()
    {
        sortOrder = sortOrder switch
        {
            "" => "Title",
            "Title" => "TitleDesc",
            _ => string.Empty
        };

        await DisplayData();
    }

    #endregion
}
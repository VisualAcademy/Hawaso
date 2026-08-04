using Microsoft.AspNetCore.Components;
using VisualAcademy.Models.Replys;

namespace Hawaso.Pages.Replys;

public partial class Index
{
    [Inject]
    public IReplyRepository RepositoryReference { get; set; }
        = default!;

    [Inject]
    public NavigationManager Nav { get; set; }
        = default!;

    // 데이터 로딩 전에는 null 상태를 사용합니다.
    protected List<Reply>? models;

    protected readonly DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    private string searchQuery = string.Empty;
    private string sortOrder = string.Empty;

    #region Lifecycle Methods

    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await DisplayData();
    }

    #endregion

    private async Task DisplayData()
    {
        var articleSet =
            await RepositoryReference.GetArticlesAsync<int>(
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
        Nav.NavigateTo($"/Replys/Details/{id}");
    }

    // DulPagerComponent의 콜백 형식과 호환되도록 async void를 유지합니다.
    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();

        StateHasChanged();
    }

    #region Search

    // SearchBox의 콜백 형식과 호환되도록 async void를 유지합니다.
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
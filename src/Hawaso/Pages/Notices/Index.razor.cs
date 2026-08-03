using Microsoft.AspNetCore.Components;
using NoticeApp.Models;

namespace Hawaso.Pages.Notices;

public class IndexComponent : ComponentBase
{
    [Inject]
    public INoticeRepository NoticeRepositoryAsyncReference { get; set; }
        = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; }
        = default!;

    // 데이터 로딩 전에는 null 상태를 사용합니다.
    protected List<Notice>? models;

    protected readonly DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 2,
        PagerButtonCount = 5
    };

    private string searchQuery = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// 검색어 유무에 따라 일반 목록 또는 검색 목록을 불러옵니다.
    /// </summary>
    private async Task LoadDataAsync()
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            await DisplayData();
        }
        else
        {
            await SearchData();
        }
    }

    /// <summary>
    /// 전체 공지사항 목록을 불러옵니다.
    /// </summary>
    private async Task DisplayData()
    {
        var resultSet =
            await NoticeRepositoryAsyncReference.GetAllAsync(
                pager.PageIndex,
                pager.PageSize);

        pager.RecordCount = resultSet.TotalRecords;
        models = resultSet.Records.ToList();
    }

    /// <summary>
    /// 검색된 공지사항 목록을 불러옵니다.
    /// </summary>
    private async Task SearchData()
    {
        var resultSet =
            await NoticeRepositoryAsyncReference.SearchAllAsync(
                pager.PageIndex,
                pager.PageSize,
                searchQuery);

        pager.RecordCount = resultSet.TotalRecords;
        models = resultSet.Records.ToList();
    }

    protected void NameClick(int id)
    {
        NavigationManagerReference.NavigateTo(
            $"/Notices/Details/{id}");
    }

    // DulPagerComponent 콜백 형식과 호환되도록 async void를 유지합니다.
    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await LoadDataAsync();

        StateHasChanged();
    }

    // SearchBox 콜백 형식과 호환되도록 async void를 유지합니다.
    protected async void Search(string query)
    {
        searchQuery = query ?? string.Empty;

        // 새로운 검색 시 첫 페이지로 이동합니다.
        pager.PageIndex = 0;
        pager.PageNumber = 1;

        await LoadDataAsync();

        StateHasChanged();
    }
}
﻿using Microsoft.AspNetCore.Components;

namespace VisualAcademy.Pages.Memos;

public partial class Index
{
    [Inject]
    public IMemoRepository RepositoryReference { get; set; }

    [Inject]
    public NavigationManager Nav { get; set; }

    protected List<Memo> models;

    protected DulPager.DulPagerBase pager = new DulPager.DulPagerBase()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    #region Lifecycle Methods
    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override async Task OnInitializedAsync() => await DisplayData();
    #endregion

    private async Task DisplayData()
    {
        var articleSet = await RepositoryReference.GetArticlesAsync<int>(pager.PageIndex, pager.PageSize, "", this.searchQuery, this.sortOrder, 0);
        pager.RecordCount = articleSet.TotalCount;
        models = articleSet.Items.ToList();

        StateHasChanged();
    }

    protected void NameClick(long id) => Nav.NavigateTo($"/Memos/Details/{id}");

    protected async void PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();

        StateHasChanged();
    }

    #region Search
    private string searchQuery = "";

    protected async void Search(string query)
    {
        pager.PageIndex = 0;

        this.searchQuery = query;

        await DisplayData();

        StateHasChanged();
    }
    #endregion

    #region Sorting
    private string sortOrder = "";

    protected async void SortByName()
    {
        if (sortOrder == "")
        {
            sortOrder = "Name";
        }
        else if (sortOrder == "Name")
        {
            sortOrder = "NameDesc";
        }
        else
        {
            sortOrder = "";
        }

        await DisplayData();
    }

    protected async void SortByTitle()
    {
        if (sortOrder == "")
        {
            sortOrder = "Title";
        }
        else if (sortOrder == "Title")
        {
            sortOrder = "TitleDesc";
        }
        else
        {
            sortOrder = "";
        }

        await DisplayData();
    }
    #endregion
}

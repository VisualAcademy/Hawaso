using Hawaso.Pages.Notices.Components;
using Microsoft.AspNetCore.Components;
using NoticeApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawaso.Pages.Notices;

public partial class Manage
{
    #region Parameters

    [Parameter]
    public int ParentId { get; set; }

    #endregion

    #region Injectors

    [Inject]
    public INoticeRepository NoticeRepositoryAsyncReference { get; set; }
        = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; }
        = default!;

    #endregion

    #region Component References

    /// <summary>
    /// EditorForm에 대한 참조:
    /// 모달을 사용하여 공지사항을 작성하거나 수정합니다.
    /// </summary>
    public EditorForm? EditorFormReference { get; set; }

    /// <summary>
    /// DeleteDialog에 대한 참조:
    /// 모달을 사용하여 공지사항을 삭제합니다.
    /// </summary>
    public DeleteDialog? DeleteDialogReference { get; set; }

    #endregion

    #region Fields

    /// <summary>
    /// 화면에 출력할 공지사항 목록입니다.
    /// 빈 컬렉션으로 초기화하여 CS8618 경고를 방지합니다.
    /// </summary>
    protected List<Notice> models = new();

    /// <summary>
    /// 현재 작성, 수정, 삭제 또는 공지 설정 대상입니다.
    /// </summary>
    protected Notice model = new();

    /// <summary>
    /// 목록 데이터를 불러오는 중인지 나타냅니다.
    /// </summary>
    protected bool isLoading = true;

    /// <summary>
    /// 현재 검색어입니다.
    /// </summary>
    private string searchQuery = string.Empty;

    /// <summary>
    /// 목록 페이징 정보입니다.
    /// </summary>
    protected DulPager.DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 2,
        PagerButtonCount = 5
    };

    #endregion

    #region Properties

    /// <summary>
    /// 공지 설정 변경 모달을 표시할지 나타냅니다.
    /// </summary>
    public bool IsInlineDialogShow { get; set; }

    /// <summary>
    /// 글쓰기 또는 수정하기 모달의 제목입니다.
    /// </summary>
    public string EditorFormTitle { get; set; } = "CREATE";

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    #endregion

    #region Data Methods

    /// <summary>
    /// 현재 검색 조건에 따라 전체 목록 또는 검색 결과를 불러옵니다.
    /// </summary>
    private async Task LoadDataAsync()
    {
        isLoading = true;

        try
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                await DisplayDataAsync();
            }
            else
            {
                await SearchDataAsync();
            }
        }
        finally
        {
            isLoading = false;
        }
    }

    /// <summary>
    /// 전체 공지사항 목록을 불러옵니다.
    /// </summary>
    private async Task DisplayDataAsync()
    {
        if (ParentId == 0)
        {
            var resultsSet =
                await NoticeRepositoryAsyncReference.GetAllAsync(
                    pager.PageIndex,
                    pager.PageSize);

            pager.RecordCount = resultsSet.TotalRecords;
            models = resultsSet.Records.ToList();
        }
        else
        {
            var resultsSet =
                await NoticeRepositoryAsyncReference.GetAllByParentIdAsync(
                    pager.PageIndex,
                    pager.PageSize,
                    ParentId);

            pager.RecordCount = resultsSet.TotalRecords;
            models = resultsSet.Records.ToList();
        }
    }

    /// <summary>
    /// 현재 검색어와 일치하는 공지사항을 불러옵니다.
    /// </summary>
    private async Task SearchDataAsync()
    {
        if (ParentId == 0)
        {
            var resultsSet =
                await NoticeRepositoryAsyncReference.SearchAllAsync(
                    pager.PageIndex,
                    pager.PageSize,
                    searchQuery);

            pager.RecordCount = resultsSet.TotalRecords;
            models = resultsSet.Records.ToList();
        }
        else
        {
            var resultsSet =
                await NoticeRepositoryAsyncReference.SearchAllByParentIdAsync(
                    pager.PageIndex,
                    pager.PageSize,
                    searchQuery,
                    ParentId);

            pager.RecordCount = resultsSet.TotalRecords;
            models = resultsSet.Records.ToList();
        }
    }

    #endregion

    #region Navigation and Paging

    protected void NameClick(int id)
    {
        NavigationManagerReference.NavigateTo(
            $"/Notices/Details/{id}");
    }

    protected async Task PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await LoadDataAsync();
    }

    #endregion

    #region Editor Event Handlers

    /// <summary>
    /// 새 공지사항 작성 모달을 표시합니다.
    /// </summary>
    protected void ShowEditorForm()
    {
        EditorFormTitle = "CREATE";
        model = new Notice();

        EditorFormReference?.Show();
    }

    /// <summary>
    /// 선택한 공지사항의 수정 모달을 표시합니다.
    /// </summary>
    protected void EditBy(Notice selectedModel)
    {
        EditorFormTitle = "EDIT";
        model = selectedModel;

        EditorFormReference?.Show();
    }

    /// <summary>
    /// 공지사항 생성 또는 수정 후 모달을 닫고 목록을 다시 불러옵니다.
    ///
    /// EditorForm의 CreateCallback과 EditCallback이 void 반환 델리게이트이므로
    /// 이 메서드는 Task가 아닌 void를 반환해야 합니다.
    /// </summary>
    protected async void CreateOrEdit()
    {
        EditorFormReference?.Hide();

        model = new Notice();

        await LoadDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    #endregion

    #region Delete Event Handlers

    /// <summary>
    /// 선택한 공지사항의 삭제 확인 모달을 표시합니다.
    /// </summary>
    protected void DeleteBy(Notice selectedModel)
    {
        model = selectedModel;

        DeleteDialogReference?.Show();
    }

    /// <summary>
    /// 선택한 공지사항을 삭제하고 목록을 다시 불러옵니다.
    /// </summary>
    protected async Task DeleteClick()
    {
        await NoticeRepositoryAsyncReference.DeleteAsync(model.Id);

        DeleteDialogReference?.Hide();

        model = new Notice();

        await LoadDataAsync();
    }

    #endregion

    #region Toggle Event Handlers

    /// <summary>
    /// 선택한 공지사항의 공지 설정 변경 모달을 표시합니다.
    /// </summary>
    protected void ToggleBy(Notice selectedModel)
    {
        model = selectedModel;
        IsInlineDialogShow = true;
    }

    /// <summary>
    /// 공지 설정 변경 모달을 닫습니다.
    /// </summary>
    protected void ToggleClose()
    {
        IsInlineDialogShow = false;
        model = new Notice();
    }

    /// <summary>
    /// 선택한 공지사항의 고정 여부를 변경합니다.
    /// </summary>
    protected async Task ToggleClick()
    {
        model.IsPinned = model.IsPinned != true;

        await NoticeRepositoryAsyncReference.EditAsync(model);

        IsInlineDialogShow = false;
        model = new Notice();

        await LoadDataAsync();
    }

    #endregion

    #region Search Event Handlers

    /// <summary>
    /// 검색어를 적용하고 첫 번째 페이지부터 검색합니다.
    /// </summary>
    protected async Task Search(string query)
    {
        searchQuery = query?.Trim() ?? string.Empty;

        pager.PageIndex = 0;
        pager.PageNumber = 1;

        await LoadDataAsync();
    }

    #endregion
}
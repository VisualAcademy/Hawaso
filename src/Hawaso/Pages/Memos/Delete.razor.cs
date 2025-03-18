using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace VisualAcademy.Pages.Memos;

public partial class Delete
{
    #region Fields
    private const string MemoListPage = "/Memos";
    #endregion

    #region Parameters
    [Parameter]
    public int Id { get; set; }
    #endregion

    #region Injectors
    [Inject]
    public IJSRuntime JSRuntimeInjector { get; set; }

    [Inject]
    public NavigationManager Nav { get; set; }

    [Inject]
    public IMemoRepository RepositoryReference { get; set; }

    [Inject]
    public IMemoFileStorageManager FileStorageManagerReference { get; set; }
    #endregion

    #region Properties
    public Memo Model { get; set; } = new Memo();

    public string Content { get; set; } = "";
    #endregion

    #region Lifecycle Methods
    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        Model = await RepositoryReference.GetByIdAsync(Id);
        Content = Dul.HtmlUtility.EncodeWithTabAndSpace(Model.Content);
    }
    #endregion

    #region Event Handlers

    /// <summary>
    /// 삭제 버튼 클릭 이벤트 처리기
    /// </summary>
    protected async void DeleteClick()
    {
        bool isDelete = await JSRuntimeInjector.InvokeAsync<bool>("confirm", $"Are you sure you want to delete it?");

        if (isDelete)
        {
            if (!string.IsNullOrEmpty(Model?.FileName))
            {
                // 첨부 파일 삭제 
                await FileStorageManagerReference.DeleteAsync(Model.FileName, "");
            }

            await RepositoryReference.DeleteAsync(Id); // 삭제
            Nav.NavigateTo(MemoListPage); // 리스트 페이지로 이동
        }
        else
        {
            await JSRuntimeInjector.InvokeAsync<object>("alert", "Canceled.");
        }
    }
    #endregion
}

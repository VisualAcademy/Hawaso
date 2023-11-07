using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using VisualAcademy.Models.Libraries;

namespace Hawaso.Pages.Libraries;

public partial class Delete
{
    #region Parameters
    [Parameter]
    public int Id { get; set; }
    #endregion

    #region Injectors
    [Inject]
    public NavigationManager NavigationManagerReference { get; set; }

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    [Inject]
    public ILibraryRepository UploadRepositoryAsyncReference { get; set; }

    [Inject]
    public ILibraryFileStorageManager FileStorageManager { get; set; }
    #endregion
    
    #region Properties
    public string Content { get; set; } = ""; 
    
    public LibraryModel Model { get; set; }
    #endregion

    #region Lifecycle Methods
    /// <summary>
    /// 페이지 초기화 이벤트 처리기
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        Model = await UploadRepositoryAsyncReference.GetByIdAsync(Id);
        Content = Dul.HtmlUtility.EncodeWithTabAndSpace(Model.Content);
    }
    #endregion

    #region Event Handlers
    protected async void DeleteClick()
    {
        bool isDelete = await JSRuntime.InvokeAsync<bool>("confirm", $"{Id}번 글을 정말로 삭제하시겠습니까?");

        if (isDelete)
        {
            if (!string.IsNullOrEmpty(Model?.FileName))
            {
                // 첨부 파일 삭제 
                await FileStorageManager.DeleteAsync(Model.FileName, "Libraries");
            }

            await UploadRepositoryAsyncReference.DeleteAsync(Id); // 삭제
            NavigationManagerReference.NavigateTo("/Libraries"); // 리스트 페이지로 이동
        }
        else
        {
            await JSRuntime.InvokeAsync<object>("alert", "취소되었습니다.");
        }
    } 
    #endregion
}

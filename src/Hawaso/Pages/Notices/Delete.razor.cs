using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NoticeApp.Models;

namespace Hawaso.Pages.Notices;

public partial class Delete
{
    [Parameter]
    public int Id { get; set; }

    [Inject]
    public INoticeRepository NoticeRepositoryAsyncReference { get; set; } = default!;

    [Inject]
    public IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManagerReference { get; set; } = default!;

    protected Notice model = new Notice();

    protected string content = "";

    protected override async Task OnInitializedAsync()
    {
        model = await NoticeRepositoryAsyncReference.GetByIdAsync(Id);
        content = Dul.HtmlUtility.EncodeWithTabAndSpace(model.Content);
    }

    protected async Task DeleteClick()
    {
        bool isDelete = await JSRuntime.InvokeAsync<bool>(
            "confirm",
            $"{Id}번 글을 정말로 삭제하시겠습니까?");

        if (isDelete)
        {
            // 데이터 삭제
            await NoticeRepositoryAsyncReference.DeleteAsync(Id);

            // 리스트 페이지로 이동
            NavigationManagerReference.NavigateTo("/Notices");
        }
        else
        {
            await JSRuntime.InvokeVoidAsync(
                "alert",
                "취소되었습니다.");
        }
    }
}
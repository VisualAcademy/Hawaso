using DulPager;
using Hawaso.Pages.Logins.Components;
using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.Logins;

public partial class Manage
{
    #region Injectors

    [Inject]
    public ILoginRepositoryAsync LoginRepositoryAsync { get; set; }
        = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; }
        = default!;

    #endregion

    private readonly DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 10,
        PagerButtonCount = 5
    };

    // 데이터 로딩 전에는 null 상태를 사용합니다.
    private List<Login>? logins;

    public string EditorFormTitle { get; set; } = "ADD";

    public Login Login { get; set; } = new();

    // @ref는 컴포넌트 렌더링 이후 할당됩니다.
    public LoginEditorForm? LoginEditorForm { get; set; }

    public LoginDeleteDialog? LoginDeleteDialog { get; set; }

    public bool IsInlineDialogShow { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await DisplayData();
    }

    private async Task DisplayData()
    {
        var articleSet = await LoginRepositoryAsync.GetAllAsync(
            pager.PageIndex,
            pager.PageSize);

        pager.RecordCount = articleSet.TotalRecords;
        logins = articleSet.Records.ToList();
    }

    private async Task PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();
    }

    protected void btnCreate_Click()
    {
        EditorFormTitle = "ADD";
        Login = new Login();

        LoginEditorForm?.Show();
    }

    // LoginEditorForm의 콜백이 void 반환 메서드를 요구하므로
    // async void 형식을 유지합니다.
    protected async void SaveOrUpdated()
    {
        LoginEditorForm?.Close();

        await DisplayData();

        StateHasChanged();
    }

    protected void EditBy(Login login)
    {
        EditorFormTitle = "EDIT";
        Login = login;

        LoginEditorForm?.Show();
    }

    protected void DeleteBy(Login login)
    {
        Login = login;

        LoginDeleteDialog?.Show();
    }

    // LoginDeleteDialog의 OnClick 콜백 형식과 호환되도록
    // async void 형식을 유지합니다.
    protected async void btnDelete_Click()
    {
        await LoginRepositoryAsync.DeleteAsync(Login.LoginId);

        LoginDeleteDialog?.Close();

        Login = new Login();

        await DisplayData();

        StateHasChanged();
    }

    protected void btnClose_Click()
    {
        IsInlineDialogShow = false;
        Login = new Login();
    }
}
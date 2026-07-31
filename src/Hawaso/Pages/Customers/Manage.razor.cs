using DotNetSaleCore.Models;
using DulPager;
using Hawaso.Pages.Customers.Components;
using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.Customers;

public partial class Manage
{
    [Inject]
    public ICustomerRepository CustomerRepositoryAsync { get; set; }
        = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; }
        = default!;

    private readonly DulPagerBase pager = new()
    {
        PageNumber = 1,
        PageIndex = 0,
        PageSize = 3,
        PagerButtonCount = 5
    };

    // 데이터가 로드되기 전에는 null 상태를 사용합니다.
    private List<Customer>? customers;

    public string EditorFormTitle { get; set; } = "ADD";

    public Customer Customer { get; set; } = new();

    // @ref는 컴포넌트가 렌더링된 이후 할당됩니다.
    public CustomerEditorForm? CustomerEditorForm { get; set; }

    public CustomerDeleteDialog? CustomerDeleteDialog { get; set; }

    public bool IsInlineDialogShow { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await DisplayData();
    }

    private async Task DisplayData()
    {
        var articleSet =
            await CustomerRepositoryAsync.GetAllAsync(
                pager.PageIndex,
                pager.PageSize);

        pager.RecordCount = articleSet.TotalRecords;
        customers = articleSet.Records.ToList();
    }

    private async Task PageIndexChanged(int pageIndex)
    {
        pager.PageIndex = pageIndex;
        pager.PageNumber = pageIndex + 1;

        await DisplayData();
    }

    private void btnCustomerName_Click(int customerId)
    {
        NavigationManager.NavigateTo(
            $"/Customers/Details/{customerId}");
    }

    protected void btnCreate_Click()
    {
        EditorFormTitle = "ADD";
        Customer = new Customer();

        CustomerEditorForm?.Show();
    }

    // CustomerEditorForm의 콜백 형식이 void 반환을 요구하므로
    // async void를 사용합니다.
    protected async void SaveOrUpdated()
    {
        CustomerEditorForm?.Close();

        await DisplayData();

        StateHasChanged();
    }

    protected void EditBy(Customer customer)
    {
        EditorFormTitle = "EDIT";
        Customer = customer;

        CustomerEditorForm?.Show();
    }

    protected void DeleteBy(Customer customer)
    {
        Customer = customer;

        CustomerDeleteDialog?.Show();
    }

    // CustomerDeleteDialog의 OnClick이 void 콜백일 가능성을 고려하여
    // 기존 콜백 형식을 유지합니다.
    protected async void btnDelete_Click()
    {
        await CustomerRepositoryAsync.DeleteAsync(
            Customer.CustomerId);

        CustomerDeleteDialog?.Close();

        Customer = new Customer();

        await DisplayData();

        StateHasChanged();
    }

    protected void ToggleBy(Customer customer)
    {
        Customer = customer;
        IsInlineDialogShow = true;
    }

    protected async Task btnToggleGender_Click()
    {
        Customer.Gender = Customer.Gender == "Male"
            ? "Female"
            : "Male";

        await CustomerRepositoryAsync.EditAsync(Customer);
        await DisplayData();

        IsInlineDialogShow = false;
    }

    protected void btnClose_Click()
    {
        IsInlineDialogShow = false;
        Customer = new Customer();
    }
}
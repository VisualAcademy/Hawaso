using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.Customers;

public partial class Edit
{
    #region Fields
    private string[] genders = { "Male", "Female" };

    private Customer customer = new Customer();
    #endregion

    #region Parameters
    [Parameter]
    public int CustomerId { get; set; }
    #endregion

    #region Injected Services
    [Inject]
    public ICustomerRepository CustomerRepositoryAsync { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        customer = await CustomerRepositoryAsync.GetByIdAsync(CustomerId);
    }
    #endregion

    #region Event Handlers
    protected async Task btnEdit_Click()
    {
        customer.Modified = DateTime.Now;

        await CustomerRepositoryAsync.EditAsync(customer);

        NavigationManager.NavigateTo($"/Customers/Details/{CustomerId}");
    }
    #endregion
}
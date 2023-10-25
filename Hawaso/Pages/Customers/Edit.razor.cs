using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Hawaso.Pages.Customers;

public partial class Edit
{
    #region Fields
    private string[] genders = { "Male", "Female" }; 
    #endregion

    #region Parameters
    [Parameter]
    public int CustomerId { get; set; }
    #endregion

    [Inject]
    public ICustomerRepository CustomerRepositoryAsync { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    private Customer customer = new Customer();

    #region Lifecycle Methods 
    protected override async Task OnInitializedAsync() => customer = await CustomerRepositoryAsync.GetByIdAsync(CustomerId);
    #endregion

    protected async Task btnEdit_Click()
    {
        customer.Modified = DateTime.Now;
        await CustomerRepositoryAsync.EditAsync(customer);
        NavigationManager.NavigateTo($"/Customers/Details/{CustomerId}");
    }
}

using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Hawaso.Pages.Customers;

public partial class Details
{
    #region Parameters
    [Parameter]
    public int CustomerId { get; set; }
    #endregion

    #region Injectors
    [Inject]
    public ICustomerRepository CustomerRepositoryReference { get; set; }
    #endregion

    #region Fields
    private Customer customer = new();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync() => customer = await CustomerRepositoryReference.GetByIdAsync(CustomerId);
    #endregion
}

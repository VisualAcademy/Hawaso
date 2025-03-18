using DotNetSaleCore.Models;
using Microsoft.AspNetCore.Components;

namespace Hawaso.Pages.Customers;

public partial class Create
{
    [Inject]
    public ICustomerRepository CustomerRepositoryReference { get; set; }

    [Inject]
    public NavigationManager Nav { get; set; }

    private Customer customer = new Customer();

    private string[] genders = { "Male", "Female" };

    protected async Task btnSubmit_Click()
    {
        customer.Created = DateTime.Now; 
        await CustomerRepositoryReference.AddAsync(customer);
        Nav.NavigateTo("/Customers");
    }
}

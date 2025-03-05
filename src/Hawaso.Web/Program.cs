using Hawaso.Web;
using Hawaso.Web.Components;
using Hawaso.Web.Components.Account;
using Hawaso.Web.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Hawaso.Web.Components.Pages.DivisionPages.Models;
using Dalbodre.Infrastructures.Cores;
using Hawaso.Web.Components.Pages.VendorPages.Models;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddRazorPages(); 
builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });


builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();



// 부서 관리: 기본 CRUD 교과서 코드
builder.Services.AddDependencyInjectionContainerForDivisionApp(connectionString);


builder.Services.AddDependencyInjectionContainerForVendorPermanentDelete(connectionString);
//builder.Services.AddDbContext<VendorPermanentDeleteDbContext>(options => { }, ServiceLifetime.Scoped);
//builder.Services.AddScoped<VendorPermanentDeleteDbContextFactory>();
//builder.Services.AddSingleton<IUserServicePermanentDelete, UserServicePermanentDelete>();
//builder.Services.AddTransient<IVendorRepositoryPermanentDelete, VendorRepositoryPermanentDelete>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();
app.MapDefaultEndpoints();

app.MapRazorPages(); // Razor Pages







#region Divisions Table Create
try
{
    DivisionTableEnsurer tableEnsurer = new DivisionTableEnsurer(connectionString);
    // Divisions 테이블이 없으면 생성
    tableEnsurer.EnsureDivisionsTableExists();
}
catch (Exception)
{

}
#endregion









app.Run();

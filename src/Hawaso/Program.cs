using All.Services;
using Azure.Identity;
using Blazored.Toast;
using Dalbodre.Infrastructures;
using Dalbodre.Infrastructures.Cores;
using DotNetNote.Models;
using DotNetSaleCore.Models;
using Hawaso.Areas.Identity;
using Hawaso.Areas.Identity.Services;
using Hawaso.Data;
using Hawaso.Extensions.BriefingLogs;
using Hawaso.Extensions.Libraries;
using Hawaso.Extensions.Memos;
using Hawaso.Infrastructures;
using Hawaso.Infrastructures.All.Identity;
using Hawaso.Infrastructures.Reports;
using Hawaso.Infrastructures.Tenants;
using Hawaso.Models.CommonValues;
using Hawaso.Models.Notes;
using Hawaso.Services;
using Hawaso.Settings;
using Hawaso.Web.Components.Pages.VendorPages.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.SemanticKernel;
using NoticeApp.Models;
using Portals.Infrastructures;
using Portals.Infrastructures.Portals.Changes;
using ReplyApp.Managers;
using VisualAcademy;
using VisualAcademy.Components.Pages.ApplicantsTransfers;
using VisualAcademy.Models.BannedTypes;
using VisualAcademy.Models.Departments;
using VisualAcademy.Models.Replys;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var services = builder.Services;
var Configuration = builder.Configuration;


builder.Services.AddHttpContextAccessor(); //[1] services.AddHttpContextAccessor();

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        Configuration.GetConnectionString("DefaultConnection"),
        options => options.EnableRetryOnFailure()));

// ASP.NET Core Identity ����
services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 4;
});

services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
});

services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, Hawaso.Areas.Identity.Services.EmailSender>();

services.AddControllersWithViews();
services.AddRazorPages();
services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

// CORS ����
services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// HttpClient ���
// HttpClient �ν��Ͻ��� DI(Dependency Injection) �����̳ʿ� ����Ͽ� ���뼺�� ����
builder.Services.AddHttpClient();

// Fluent UI Blazor library add: �ݵ�� AddHttpClient() Ȯ�� �޼��� ������ ��ġ�� ��
builder.Services.AddFluentUIComponents();



builder.Services.AddScoped<ApplicantUploadService>();



services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
services.AddDatabaseDeveloperPageExceptionFilter();
services.AddSingleton<WeatherForecastService>();

services.Configure<DotNetNoteSettings>(Configuration.GetSection("DotNetNoteSettings"));

services.AddDbContext<DotNetNoteContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
services.AddDbContext<NoteDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

#region Changes ���̺� ���� 
//var schemaEnhancerChanges = new TenantSchemaEnhancerCreateChangesTable(connectionString);
//schemaEnhancerChanges.CreateChangesTable();
#endregion




#region AspNetUsers ���̺� ���ο� �÷� �߰� 
var aspNetUsersTableAddColumn = new AspNetUsersTableEnhancer(Configuration.GetConnectionString("DefaultConnection"));
aspNetUsersTableAddColumn.EnsureColumnsExist();
#endregion



#region DailyLogs ���̺� ���� �� �÷� Ȯ��
var dailyLogsTableEnhancer = new DailyLogsTableEnhancer(Configuration.GetConnectionString("DefaultConnection"));
dailyLogsTableEnhancer.EnsureDailyLogsTable();
#endregion



// ������ ���� �����̳� ����
DependencyInjectionContainer(services);

services.AddTransient<IFileStorageManager, ReplyAppFileStorageManager>();

services.AddDependencyInjectionContainerForPurgeApp(Configuration.GetConnectionString("DefaultConnection"));

// ������ ���� �޼��� ����
void DependencyInjectionContainer(IServiceCollection services)
{
    services.AddSingleton<IConfiguration>(Configuration);

    services.AddTransient<INoteRepository, NoteRepository>();
    services.AddTransient<INoteCommentRepository, NoteCommentRepository>();

    services.AddDbContext<HawasoDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);
    services.AddDbContext<CommonValueDbContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
    services.AddTransient<ICommonValueRepository, CommonValueRepository>();

    // ManufacturerApp ���� ������ ����
    services.AddDependencyInjectionContainerForManufacturer(Configuration.GetConnectionString("DefaultConnection"));

    // Blazored.Toast
    services.AddBlazoredToast();

    // MemoApp ���� ������ ����
    services.AddDependencyInjectionContainerForMemoApp(Configuration.GetConnectionString("DefaultConnection"));

    // Upload Feature
    services.AddDiForLibries(Configuration.GetConnectionString("DefaultConnection"));
    services.AddDiForBriefingLogs(Configuration.GetConnectionString("DefaultConnection"));

    // ArchiveApp ���� ������ ����
    services.AddDependencyInjectionContainerForArchiveApp(Configuration.GetConnectionString("DefaultConnection"));

    #region VisualAcademy.Models.Departments.dll 
    // DepartmentApp ���� ������ ����
    services.AddDependencyInjectionContainerForDepartmentApp(Configuration.GetConnectionString("DefaultConnection"));
    #endregion

    builder.Services.AddDependencyInjectionContainerForBannedTypeApp(connectionString);

    /// <summary>
    /// ��������(NoticeApp) ���� ������(���Ӽ�) ���� ���� �ڵ常 ���� ��Ƽ� ���� 
    /// </summary>
    services.AddDependencyInjectionContainerForNoticeApp(Configuration["ConnectionStrings:DefaultConnection"]); // �� �ٸ� �����ͺ��̽� ���� ���ڿ� ǥ����
}

// DotNetSaleCore ���� ������ ����
AddDependencyInjectionContainerForDotNetSaleCore(services, Configuration);

// �����ͺ��̽� �ʱ�ȭ �� ���̱׷��̼�
try
{
    await DatabaseHelper.AddOrUpdateRegistrationDate(connectionString);
}
catch (Exception)
{

}




#region Semantic Kernel
builder.Services.AddKernel();

var aiConfig = builder.Configuration.GetSection("SmartComponents");
//builder.Services.AddAzureOpenAIChatCompletion(
//    deploymentName: aiConfig["DeploymentName"],
//    endpoint: aiConfig["Endpoint"],
//    new DefaultAzureCredential()); 
#endregion





services.AddDependencyInjectionContainerForVendorPermanentDelete(connectionString);



services.AddTransient<ITwilioSender, TwilioSender>();


services.AddTransient<IMailchimpEmailSender, MailchimpEmailSender>();


var app = builder.Build();





if (app.Environment.IsProduction())
{ 
    #region Create Changes table and add more columns
    var changesTableEnhancer = new ChangesTableSchemaEnhancer(connectionString);

    // ���̺� ����
    changesTableEnhancer.CreateChangesTable();
    // �÷� �߰�
    changesTableEnhancer.AddTenantNameColumnIfNotExists();
    #endregion
}







// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

#region Tenants Update
// Enhance tenant databases on startup
//EnhanceTenantDatabases(app.Services, app.Configuration); 
#endregion

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAnyOrigin");

// Configure the HTTP request pipeline.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapRazorPages();

#region CreateBuiltInUsersAndRoles and ResetAdministratorPassword
// �⺻ ���� �� ����� �߰� �� ������ ��ȣ �ʱ�ȭ 
using (var scope = app.Services.CreateScope())
{
    var scopedServices = scope.ServiceProvider;
    var configuration = scopedServices.GetRequiredService<IConfiguration>();

    // ���� ����� �� ���� ����
    await UserAndRoleInitializer.CreateBuiltInUsersAndRoles(scopedServices);

    // ������ ���� ��ȣ �ʱ�ȭ ���� Ȯ��
    bool resetPassword = configuration.GetValue<bool>("PasswordReset:ResetAdministratorPassword");
    if (resetPassword)
    {
        await AdministratorPasswordResetHelper.ResetAdministratorPassword(scopedServices, "hawaso.com");
    }
}
#endregion

#region Tenants Table ���� �� �÷� �߰� ����
var tenantSchemaEnhancerCreateAndAlter = new TenantSchemaEnhancerCreateAndAlter(Configuration.GetConnectionString("DefaultConnection"));
tenantSchemaEnhancerCreateAndAlter.EnsureSchema();
#endregion

// **PageSchemaEnhancer** �ν��Ͻ� ����
PageSchemaEnhancer pageSchemaEnhancer = new PageSchemaEnhancer(connectionString);
pageSchemaEnhancer.EnsurePagesTableExists();

#region Partners Table ���� �� �÷� �߰� ����
var tenantSchemaEnhancerCreatePartnersTable = new TenantSchemaEnhancerCreatePartnersTable(Configuration.GetConnectionString("DefaultConnection"));
tenantSchemaEnhancerCreatePartnersTable.EnhanceAllTenantDatabases();
#endregion







#region Create ApplicantsTransfers Table
if (DateTime.Now < (new DateTime(2025, 2, 10)))
{
    var createTenantsTransfersTable = new TenantSchemaEnhancerCreateApplicantsTransfersTable(connectionString);
    createTenantsTransfersTable.CreateApplicantsTransfersTable();
}
#endregion








app.Run();

/// <summary>
/// ���θ�(DotNetSaleCore) ���� ������(���Ӽ�) ���� ���� �ڵ常 ���� ��Ƽ� ����
/// </summary>
void AddDependencyInjectionContainerForDotNetSaleCore(IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<DotNetSaleCoreDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);

    services.AddTransient<ICustomerRepository, CustomerRepository>();
    services.AddTransient<ICategoryRepository, CategoryRepository>();
    services.AddTransient<IProductRepositoryAsync, ProductRepositoryAsync>();

    services.AddSingleton<ILoginRepository>(
        new LoginRepository(configuration.GetConnectionString("DefaultConnection")));
    services.AddDbContext<LoginDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);
    services.AddTransient<ILoginRepositoryAsync, LoginRepositoryAsync>();
}

void EnhanceTenantDatabases(IServiceProvider services, IConfiguration configuration)
{
    var masterConnectionString = configuration.GetConnectionString("DefaultConnection");
    var schemaEnhancerApplications = new TenantSchemaEnhancerAddColumnApplications(masterConnectionString);
    schemaEnhancerApplications.EnhanceAllTenantDatabases();
}

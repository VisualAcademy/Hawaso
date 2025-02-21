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

// ASP.NET Core Identity 설정
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

// CORS 설정
services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// HttpClient 등록
// HttpClient 인스턴스를 DI(Dependency Injection) 컨테이너에 등록하여 재사용성을 높임
builder.Services.AddHttpClient();

// Fluent UI Blazor library add: 반드시 AddHttpClient() 확장 메서드 다음에 위치할 것
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

#region Changes 테이블 생성 
//var schemaEnhancerChanges = new TenantSchemaEnhancerCreateChangesTable(connectionString);
//schemaEnhancerChanges.CreateChangesTable();
#endregion




#region AspNetUsers 테이블에 새로운 컬럼 추가 
var aspNetUsersTableAddColumn = new AspNetUsersTableEnhancer(Configuration.GetConnectionString("DefaultConnection"));
aspNetUsersTableAddColumn.EnsureColumnsExist();
#endregion



#region DailyLogs 테이블 생성 및 컬럼 확인
var dailyLogsTableEnhancer = new DailyLogsTableEnhancer(Configuration.GetConnectionString("DefaultConnection"));
dailyLogsTableEnhancer.EnsureDailyLogsTable();
#endregion



// 의존성 주입 컨테이너 설정
DependencyInjectionContainer(services);

services.AddTransient<IFileStorageManager, ReplyAppFileStorageManager>();

services.AddDependencyInjectionContainerForPurgeApp(Configuration.GetConnectionString("DefaultConnection"));

// 의존성 주입 메서드 정의
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

    // ManufacturerApp 관련 의존성 주입
    services.AddDependencyInjectionContainerForManufacturer(Configuration.GetConnectionString("DefaultConnection"));

    // Blazored.Toast
    services.AddBlazoredToast();

    // MemoApp 관련 의존성 주입
    services.AddDependencyInjectionContainerForMemoApp(Configuration.GetConnectionString("DefaultConnection"));

    // Upload Feature
    services.AddDiForLibries(Configuration.GetConnectionString("DefaultConnection"));
    services.AddDiForBriefingLogs(Configuration.GetConnectionString("DefaultConnection"));

    // ArchiveApp 관련 의존성 주입
    services.AddDependencyInjectionContainerForArchiveApp(Configuration.GetConnectionString("DefaultConnection"));

    #region VisualAcademy.Models.Departments.dll 
    // DepartmentApp 관련 의존성 주입
    services.AddDependencyInjectionContainerForDepartmentApp(Configuration.GetConnectionString("DefaultConnection"));
    #endregion

    builder.Services.AddDependencyInjectionContainerForBannedTypeApp(connectionString);

    /// <summary>
    /// 공지사항(NoticeApp) 관련 의존성(종속성) 주입 관련 코드만 따로 모아서 관리 
    /// </summary>
    services.AddDependencyInjectionContainerForNoticeApp(Configuration["ConnectionStrings:DefaultConnection"]); // 또 다른 데이터베이스 연결 문자열 표현법
}

// DotNetSaleCore 관련 의존성 주입
AddDependencyInjectionContainerForDotNetSaleCore(services, Configuration);

// 데이터베이스 초기화 및 마이그레이션
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

    // 테이블 생성
    changesTableEnhancer.CreateChangesTable();
    // 컬럼 추가
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
// 기본 역할 및 사용자 추가 및 관리자 암호 초기화 
using (var scope = app.Services.CreateScope())
{
    var scopedServices = scope.ServiceProvider;
    var configuration = scopedServices.GetRequiredService<IConfiguration>();

    // 내장 사용자 및 역할 생성
    await UserAndRoleInitializer.CreateBuiltInUsersAndRoles(scopedServices);

    // 관리자 계정 암호 초기화 여부 확인
    bool resetPassword = configuration.GetValue<bool>("PasswordReset:ResetAdministratorPassword");
    if (resetPassword)
    {
        await AdministratorPasswordResetHelper.ResetAdministratorPassword(scopedServices, "hawaso.com");
    }
}
#endregion

#region Tenants Table 생성 및 컬럼 추가 데모
var tenantSchemaEnhancerCreateAndAlter = new TenantSchemaEnhancerCreateAndAlter(Configuration.GetConnectionString("DefaultConnection"));
tenantSchemaEnhancerCreateAndAlter.EnsureSchema();
#endregion

// **PageSchemaEnhancer** 인스턴스 생성
PageSchemaEnhancer pageSchemaEnhancer = new PageSchemaEnhancer(connectionString);
pageSchemaEnhancer.EnsurePagesTableExists();

#region Partners Table 생성 및 컬럼 추가 데모
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
/// 쇼핑몰(DotNetSaleCore) 관련 의존성(종속성) 주입 관련 코드만 따로 모아서 관리
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

using Azunt.EmployeeManagement;
using Azunt.Endpoints;
using Azunt.FileManagement;
using Azunt.Infrastructures;
using Azunt.Models.Enums;
using Azunt.NoteManagement;
using Azunt.ReasonManagement;
using Azunt.ResourceManagement;
using Azunt.Services;
using Azunt.Web.Components.Pages.Notes.Services;
using Azunt.Web.Policies;
using Azunt.Web.Settings;
using Blazored.Toast;
using Dalbodre.Infrastructures.Cores;
using DotNetNote.Models;
using DotNetSaleCore.Models;
using Hawaso.Areas.Identity;
using Hawaso.Endpoints;
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
using Microsoft.Extensions.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using NoticeApp.Models;
using ReplyApp.Managers;
using System.Net.Http.Headers;
using System.Security.Claims;
using VisualAcademy;
using VisualAcademy.Components.Pages.ApplicantsTransfers;
using VisualAcademy.Models.BannedTypes;
using VisualAcademy.Models.Departments;
using VisualAcademy.Models.Replys;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");




#region GlobalAdministrators Policy 설정
// GlobalAdministrators 이메일 목록 읽기
// - appsettings.json의 "AuthorizationSettings:GlobalAdministrators" 배열을 읽어옵니다.
// - 값이 없으면 빈 리스트로 초기화하여 NullReferenceException을 방지합니다.
var globalAdminEmails = builder.Configuration.GetSection("AuthorizationSettings:GlobalAdministrators").Get<List<string>>() ?? new List<string>();
#endregion




#region Authorization Policy Configuration
// 정책 기반 권한 설정
// 참조: https://dul.me/docs/aspnet/core/security/add-authorization-policies/

// 애플리케이션의 권한 정책(Policy)들을 정의합니다.
builder.Services.AddAuthorization(options =>
{
    // "AdminOnly" 정책:
    // - "Administrators" 역할을 가진 사용자만 접근 허용
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Administrators"));

    // "ManagerOnly" 정책:
    // - "Managers" 역할을 가진 사용자만 접근 허용
    options.AddPolicy("ManagerOnly", policy =>
        policy.RequireRole("Managers"));

    #region GlobalAdministrators Policy 설정
    // "GlobalAdministrators" 정책 정의
    // - 조건 1: "Administrators" 역할(Role)에 속해야 함
    // - 조건 2: 이메일(ClaimTypes.Email)이 globalAdminEmails 리스트에 포함되어야 함
    options.AddPolicy("GlobalAdministrators", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Administrators") && // 역할 검사
            context.User.HasClaim(c =>
                c.Type == ClaimTypes.Email && // 이메일 Claim 존재 여부 검사
                globalAdminEmails.Contains(c.Value, StringComparer.OrdinalIgnoreCase)) // 이메일 리스트 포함 여부 검사
        )
    );
    #endregion

});
#endregion




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
services.AddDbContext<DotNetNote.Models.NoteDbContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

#region Changes 테이블 생성 
//var schemaEnhancerChanges = new TenantSchemaEnhancerCreateChangesTable(connectionString);
//schemaEnhancerChanges.CreateChangesTable();
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

    services.AddTransient<Hawaso.Models.Notes.INoteRepository, Hawaso.Models.Notes.NoteRepository>();
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

    services.AddDependencyInjectionContainerForReasonApp(connectionString, RepositoryMode.EfCore);
    services.AddTransient<ReasonAppDbContextFactory>();


    #region ResourceManagement 
    // Resource 모듈 등록
    services.AddDependencyInjectionContainerForResourceApp(connectionString, Azunt.Models.Enums.RepositoryMode.EfCore);
    services.AddTransient<ResourceAppDbContextFactory>();
    #endregion
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



var defaultConnStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is missing in configuration.");

builder.Services.AddDependencyInjectionContainerForFileApp(defaultConnStr);
builder.Services.AddTransient<FileAppDbContextFactory>();

builder.Services.AddScoped<IFileStorageService, Azunt.Web.Components.Pages.FilesPages.Services.AzureBlobStorageService>();



#region NoteManagement
builder.Services.AddDependencyInjectionContainerForNoteApp(connectionString, Azunt.Models.Enums.RepositoryMode.EfCore);
builder.Services.AddTransient<Azunt.NoteManagement.NoteDbContextFactory>();
builder.Services.AddScoped<Azunt.NoteManagement.INoteStorageService, NoOpNoteStorageService>();
#endregion



// 최신 권장 방식: HttpClientFactory 등록
builder.Services.AddHttpClient("egress-ip", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Azunt-EgressIp/1.0");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
});


#region Background Service
// appsettings.json 바인딩
builder.Services.Configure<BackgroundScreeningOptions>(
    builder.Configuration.GetSection("BackgroundScreening"));

// 정책 서비스 DI 
builder.Services.AddScoped<IBackgroundScreeningPolicy, BackgroundScreeningPolicy>();
#endregion


var app = builder.Build();





//if (app.Environment.IsProduction())
{
    #region Create Changes table and add more columns
    var changesTableEnhancer = new ChangesTableSchemaEnhancer(connectionString);

    // 테이블 생성 및 컬럼 추가
    changesTableEnhancer.EnhanceChangesTable();
    #endregion
}




try
{
    var documentsTableEnhancer = new DocumentsTableEnhancer(connectionString);
    await documentsTableEnhancer.EnsureDocumentsTableColumnsAsync();
}
catch (Exception)
{

}





// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // 브라우저에 HTTPS만 사용하도록 지시하는 보안 헤더 추가
}

app.UseHttpsRedirection(); // HTTP 요청을 HTTPS로 자동 리디렉션

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











#region 데이터베이스 및 인증 스키마 초기화
var config = app.Services.GetRequiredService<IConfiguration>();
bool initializeDatabase = config.GetValue<bool>("Database:InitializeOnStartup");

if (initializeDatabase)
{
    DatabaseInitializer.Initialize(app.Services);
}
else
{
    Console.WriteLine("Database initialization is skipped (Database:InitializeOnStartup = false)");
}
#endregion












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




// **PageSchemaEnhancer** 인스턴스 생성
PageSchemaEnhancer pageSchemaEnhancer = new PageSchemaEnhancer(connectionString);
pageSchemaEnhancer.EnsurePagesTableExists();

//#region Partners Table 생성 및 컬럼 추가 데모
//var tenantSchemaEnhancerCreatePartnersTable = new TenantSchemaEnhancerCreatePartnersTable(Configuration.GetConnectionString("DefaultConnection"));
//tenantSchemaEnhancerCreatePartnersTable.EnhanceAllTenantDatabases();
//#endregion


#region Create ApplicantsTransfers Table
if (DateTime.Now < (new DateTime(2025, 2, 10)))
{
    var createTenantsTransfersTable = new TenantSchemaEnhancerCreateApplicantsTransfersTable(connectionString);
    createTenantsTransfersTable.CreateApplicantsTransfersTable();
}
#endregion


// Create AllowedIPRanges table in the default database
var defaultSchemaEnhancer = new All.Infrastructures.Cores.DefaultSchemaEnhancerCreateAllowedIPRangesTable(Configuration.GetConnectionString("DefaultConnection"));
defaultSchemaEnhancer.EnhanceDefaultDatabase();



//using (var scope = app.Services.CreateScope())
//{
//    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
//    string masterConnectionString = configuration.GetConnectionString("DefaultConnection");

//    var enhancer = new TenantSchemaEnhancerCreateCustomFieldTitlesTable(masterConnectionString);
//    enhancer.EnhanceAllTenantDatabases();
//}



#region Minimal API - GET /api/foo

// minimal-api-getting-started Program 파일에서 Foo 이름의 Minimal API 만들고 테스트하기: https://youtu.be/DROgDGtGdYU
app.MapGet("api/foo", () =>
{
    return new[]
    {
        new { Id = 1, Name = "Foo" },
        new { Id = 2, Name = "Bar" }
    };
});

#endregion



// 엔드포인트 등록
app.MapIsoCountriesEndpoint();


// Diagnostics 엔드포인트 매핑
app!.MapDiagnosticsEndpoints();







#region Employees 테이블 초기화/보강 및 시드
try
{
    var cfg = app.Services.GetRequiredService<IConfiguration>();
    var employeesSection = cfg.GetSection("Database:Initializers")
                              .GetChildren()
                              .FirstOrDefault(x =>
                                  string.Equals(x["Name"], "Employees", StringComparison.OrdinalIgnoreCase));

    if (employeesSection != null)
    {
        bool forMaster = bool.TryParse(employeesSection["ForMaster"], out var fm) ? fm : false;
        bool enableSeeding = bool.TryParse(employeesSection["EnableSeeding"], out var es) ? es : false;

        EmployeesTableBuilder.Run(app.Services, forMaster: forMaster, enableSeeding: enableSeeding);

        Console.WriteLine(
            $"Employees table initialization finished. Target={(forMaster ? "Master" : "Tenants")}, Seed={enableSeeding}"
        );
    }
    else
    {
        Console.WriteLine("Employees initializer not configured in Database:Initializers. Skipped.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Employees table initialization failed: {ex.Message}");
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

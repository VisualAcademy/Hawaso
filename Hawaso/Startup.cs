using Blazored.Toast;
using DotNetNote.Models;
using DotNetSaleCore.Models;
using Hawaso.Areas.Identity;
using Hawaso.Areas.Identity.Services;
using Hawaso.Data;
using Hawaso.Extensions.BriefingLogs;
using Hawaso.Extensions.Memos;
using Hawaso.Models.CommonValues;
using Hawaso.Models.Notes;
using Hawaso.Rules;
using Hawaso.Settings;
using MachineTypeApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NoticeApp.Models;
using ReplyApp.Managers;
using System;
using System.Linq;
using System.Threading.Tasks;
using VisualAcademy.Models.Replys;
using Zero.Models;

namespace Hawaso;

public class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddHttpContextAccessor(); //[1]

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), options => options.EnableRetryOnFailure()));


        #region 인증과 권한 설정: ASP.NET Core Identity
        // ASP.NET Core Identity 인증과 권한 

        //[!] ApplicationUser 클래스로 사용자 관리, ApplicationRole 클래스로 역할 관리
        // ____ (IdentityUser 클래스와 IdentityRole 클래스가 기본값)

        ////[!] 기본 템플릿 원본 모양: 참고용으로 추가
        //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

        //[A] ASP.NET Core Identity 확장
        services.AddIdentity<ApplicationUser, ApplicationRole>(
            options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                //options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        //// .NET 6 
        //builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => options.SignIn.RequireConfirmedAccount = true)
        //    .AddEntityFrameworkStores<ApplicationDbContext>()
        //    .AddDefaultTokenProviders();

        //[B] Identity 옵션 설정
        services.Configure<IdentityOptions>(options =>
        {
            // 암호 설정
            options.Password.RequiredLength = 4;
            //options.Password.RequireDigit = true;
            //options.Password.RequireLowercase = true;
            //options.Password.RequireUppercase = false;
            //options.Password.RequireNonAlphanumeric = false;

            //// 잠금 설정
            //options.Lockout.MaxFailedAccessAttempts = 5;
            //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

            //// 사용자 설정
            //options.User.RequireUniqueEmail = true;
        });

        //[C] 로그인 페이지 경로 재 설정 
        services.ConfigureApplicationCookie(options =>
        {
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            //options.Cookie.Name = "YourAppCookieName";
            //options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(15);

            //options.LoginPath = "/Account/LogIn";
            options.LoginPath = "/Identity/Account/Login";

            //options.LogoutPath = "/Account/LogOff";
            options.LogoutPath = "/Identity/Account/Logout";

            // ReturnUrlParameter requires 
            //using Microsoft.AspNetCore.Authentication.Cookies;
            options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
            options.SlidingExpiration = true;
        });

        //[D] EmailSender 주입
        services.AddTransient<IEmailSender, EmailSender>();

        #endregion


        //[!] MVC 사용을 위한 서비스 등록: 가장 기본적인 확장 메서드
        services.AddControllersWithViews();     // MVC + Web API 사용 가능
        services.AddRazorPages();               // Razor Pages 사용 가능
        services.AddServerSideBlazor()          // Blazor Server 사용 가능
            .AddCircuitOptions(options => { options.DetailedErrors = true; }); // 자세한 에러 출력 

        #region CORS
        //[CORS][1] CORS 사용 등록
        //[CORS][1][1] 기본: 모두 허용
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAnyOrigin",
                builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        });
        //[CORS][1][2] 참고: 모두 허용
        services.AddCors(o => o.AddPolicy("AllowAllPolicy", options =>
        {
            options.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        }));
        //[CORS][1][3] 참고: 특정 도메인만 허용
        services.AddCors(o => o.AddPolicy("AllowSpecific", options =>
                options.WithOrigins("https://localhost:44356")
                       .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
                       .WithHeaders("accept", "content-type", "origin", "X-TotalRecordCount")));
        #endregion




        //[1] HttpClient 사용하기 - 설정
        services.AddHttpClient();


        //[!] 기본 템플릿 제공 서비스 등록 코드 
        //services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
        services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddSingleton<WeatherForecastService>();


        //[!] Configuration: JSON 파일의 데이터를 POCO 클래스에 주입
        services.Configure<DotNetNoteSettings>(Configuration.GetSection("DotNetNoteSettings"));


        // ============================================================================== // 
        // 새로운 DbContext 추가
        //services.AddEntityFrameworkSqlServer().AddDbContext<DotNetNoteContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        services.AddDbContext<DotNetNoteContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        services.AddDbContext<NoteDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        // ============================================================================== // 





        //[DI] 의존성 주입(Dependency Injection)
        DependencyInjectionContainer(services);

        AddDependencyInjectionContainerForDotNetSaleCore(services);

        /// <summary>
        /// 공지사항(NoticeApp) 관련 의존성(종속성) 주입 관련 코드만 따로 모아서 관리 
        /// </summary>
        services.AddDependencyInjectionContainerForNoticeApp(Configuration["ConnectionStrings:DefaultConnection"]); // 또 다른 데이터베이스 연결 문자열 표현법

        // MachineTypeApp 관련 의존성(종속성) 주입 관련 코드만 따로 모아서 관리 
        services.AddDependencyInjectionContainerForMachineTypeApp(Configuration.GetConnectionString("DefaultConnection"));



        AddDependencyInjectionContainerForUploadApp(services);
        AddDependencyInjectionContainerForReplyApp(services);


        services.AddTransient<IFileStorageManager, ReplyAppFileStorageManager>(); // Local Upload

        //// Blazor Server에서 용량이 큰 파일 업로드 설정
        //services.AddSignalR(e =>
        //{
        //    e.MaximumReceiveMessageSize = 102400000;
        //});

        services.AddDependencyInjectionContainerForPurgeApp(Configuration.GetConnectionString("DefaultConnection")); // Test
    }

    private void AddDependencyInjectionContainerForUploadApp(IServiceCollection services)
    {
        services.AddDbContext<UploadAppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);
        services.AddTransient<IUploadRepository, UploadRepository>();
    }

    private void AddDependencyInjectionContainerForReplyApp(IServiceCollection services)
    {
        services.AddDbContext<ReplyAppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);
        services.AddTransient<IReplyRepository, ReplyRepository>();
    }

    /// <summary>
    /// 쇼핑몰(DotNetSaleCore) 관련 의존성(종속성) 주입 관련 코드만 따로 모아서 관리
    /// </summary>
    private void AddDependencyInjectionContainerForDotNetSaleCore(IServiceCollection services)
    {
        services.AddDbContext<DotNetSaleCoreDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);

        // ICustomerRepositoryAsync.cs Inject: DI Container에 서비스(리포지토리) 등록
        services.AddTransient<ICustomerRepository, CustomerRepository>();
        services.AddTransient<ICategoryRepository, CategoryRepository>(); //[1]
        services.AddTransient<IProductRepositoryAsync, ProductRepositoryAsync>(); //[2]

        services.AddSingleton<ILoginRepository>(
            new LoginRepository(Configuration.GetConnectionString("DefaultConnection")));
        services.AddDbContext<LoginDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);
        services.AddTransient<ILoginRepositoryAsync, LoginRepositoryAsync>(); //[1]
    }

    /// <summary>
    /// 의존성 주입 관련 코드만 따로 모아서 관리
    /// - 리포지토리 등록
    /// </summary>
    private void DependencyInjectionContainer(IServiceCollection services)
    {
        //[?] ConfigureServices가 호출되기 전에는 DI(종속성 주입)가 설정되지 않습니다.

        //[DNN][!] Configuration 개체 주입: 
        //    IConfiguration 또는 IConfigurationRoot에 Configuration 개체 전달
        //    appsettings.json 파일의 데이터베이스 연결 문자열을 
        //    리포지토리 클래스에서 사용할 수 있도록 설정
        // IConfiguration 주입 -> Configuration의 인스턴스를 실행 
        services.AddSingleton<IConfiguration>(Configuration);

        //[DNN][1] 게시판 관련 서비스 등록            
        //[1] 생성자에 문자열로 연결 문자열 지정
        //services.AddSingleton<INoteRepository>(
        //  new NoteRepository(
        //      Configuration["Data:DefaultConnection:ConnectionString"]));            
        //[2] 의존성 주입으로 Configuration 주입
        //[a] NoteRepository에서 IConfiguration으로 데이터베이스 연결 문자열 접근
        services.AddTransient<INoteRepository, NoteRepository>();
        //[b] CommentRepository의 생성자에 데이터베이스 연결문자열 직접 전송
        //services.AddSingleton<INoteCommentRepository>(
        //    new NoteCommentRepository(
        //        Configuration["ConnectionStrings:DefaultConnection"]));
        //[b] 위 코드를 아래 코드로 변경
        services.AddTransient<INoteCommentRepository, NoteCommentRepository>();

        // HawasoDbContext 주입
        // DbContext ServiceLifetime 옵션은 모두 Transient를 주었습니다. 
        services.AddDbContext<HawasoDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);
        services
            .AddDbContext<CommonValueDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        services.AddTransient<ICommonValueRepository, CommonValueRepository>(); // CommonValues

        // 고객사앱(ManufacturerApp) 관련 의존성(종속성) 주입 관련 코드만 따로 모아서 관리
        services.AddDependencyInjectionContainerForManufacturer(Configuration.GetConnectionString("DefaultConnection"));
        services.AddBlazoredToast(); // Blazored.Toast

        // 부서 관리: 기본 CRUD 교과서 코드
        services.AddDependencyInjectionContainerForDepartmentApp(Configuration.GetConnectionString("DefaultConnection"));

        /// <summary>
        /// 메모앱(MemoApp) 관련 의존성(종속성) 주입 관련 코드만 따로 모아서 관리: 게시판 및 CRUD 관련 교과서 코드 
        /// </summary>
        services.AddDependencyInjectionContainerForMemoApp(Configuration.GetConnectionString("DefaultConnection"));

        // Upload Feature
        services.AddDiForLibries(Configuration.GetConnectionString("DefaultConnection"));
        services.AddDiForBriefingLogs(Configuration.GetConnectionString("DefaultConnection"));

        // Memos -> Archives
        services.AddDependencyInjectionContainerForArchiveApp(Configuration.GetConnectionString("DefaultConnection"));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            //app.UseDatabaseErrorPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseDefaultFiles(); // index.html 문서 실행 
        app.UseStaticFiles();

        app.UseRouting();




        #region Azure Web App 고유 경로 요청시 www.hawaso.com 경로로 영구 이동
        // Azure Web App 고유 경로 요청시 www.hawaso.com 경로로 영구 이동 
        app.UseRewriter(new RewriteOptions().Add(new RedirectAzureWebsitesRule()).AddRedirectToWwwPermanent());
        #endregion



        app.UseAuthentication();
        app.UseAuthorization();


        #region CORS
        //[CORS][2] CORS 사용 허용
        app.UseCors("AllowAnyOrigin");
        #endregion


        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapRazorPages();

            ////[!] 루트 페이지 로드하자마자 특정 URL로 이동하고자 할 때 
            //endpoints.MapGet("/", context =>
            //{
            //    //[!] "/" 경로 요청 시 MVC Home Controller의 Index 액션 메서드 실행
            //    //____나중에 더 좋은 로직 발견하면 여기 코드 대체 예정
            //    context.Response.Redirect("/Home"); 
            //    return Task.CompletedTask;
            //});
        });

        // ASP.NET Core Identity 기본 사용자 및 역할 생성 
        CreateBuiltInUsersAndRoles(serviceProvider).Wait();
    }

    #region Create BuiltIn Users and Roles 
    /// <summary>
    /// 내장 사용자 및 그룹(역할) 생성
    /// </summary>
    private async Task CreateBuiltInUsersAndRoles(IServiceProvider serviceProvider)
    {
        //[0] DbContext 개체 생성
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated(); // 데이터베이스가 생성되어 있는지 확인 

        // 기본 내장 사용자 및 역할이 하나도 없으면(즉, 처음 데이터베이스 생성이라면)
        if (!dbContext.Users.Any() && !dbContext.Roles.Any())
        {
            string domainName = "hawaso.com";

            //[1] Groups(Roles)
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            //[1][1] ('Administrators', '관리자 그룹', 'Group', '응용 프로그램을 총 관리하는 관리 그룹 계정')
            //[1][2] ('Everyone', '전체 사용자 그룹', 'Group', '응용 프로그램을 사용하는 모든 사용자 그룹 계정')
            //[1][3] ('Users', '일반 사용자 그룹', 'Group', '일반 사용자 그룹 계정')
            //[1][4] ('Guests', '게스트 그룹', 'Group', '게스트 사용자 그룹 계정')
            //string[] roleNames = { Dul.Roles.Administrators.ToString(), Dul.Roles.Everyone.ToString(), Dul.Roles.Users.ToString(), Dul.Roles.Guests.ToString() };
            //foreach (var roleName in roleNames)
            //{
            //    var roleExist = await roleManager.RoleExistsAsync(roleName);
            //    if (!roleExist)
            //    {
            //        await roleManager.CreateAsync(new ApplicationRole { Name = roleName, NormalizedName = roleName.ToUpper(), Description = "" }); // 빌트인 그룹 생성
            //    }
            //}
            //[1][1] Administrators
            var administrators = new ApplicationRole 
            { 
                Name = Dul.Roles.Administrators.ToString(), 
                NormalizedName = Dul.Roles.Administrators.ToString().ToUpper(), 
                Description = "응용 프로그램을 총 관리하는 관리 그룹 계정" 
            };
            if (!(await roleManager.RoleExistsAsync(administrators.Name)))
            {
                await roleManager.CreateAsync(administrators);
            }
            //[1][2] Everyone
            var everyone = new ApplicationRole 
            { 
                Name = Dul.Roles.Everyone.ToString(), 
                NormalizedName = Dul.Roles.Everyone.ToString().ToUpper(), 
                Description = "응용 프로그램을 사용하는 모든 사용자 그룹 계정" 
            };
            if (!(await roleManager.RoleExistsAsync(everyone.Name)))
            {
                await roleManager.CreateAsync(everyone);
            }
            //[1][3] Users
            var users = new ApplicationRole 
            { 
                Name = Dul.Roles.Users.ToString(), 
                NormalizedName = Dul.Roles.Users.ToString().ToUpper(), 
                Description = "일반 사용자 그룹 계정" 
            };
            if (!(await roleManager.RoleExistsAsync(users.Name)))
            {
                await roleManager.CreateAsync(users);
            }
            //[1][4] Guests
            var guests = new ApplicationRole 
            { 
                Name = Dul.Roles.Guests.ToString(), 
                NormalizedName = Dul.Roles.Guests.ToString().ToUpper(), 
                Description = "게스트 사용자 그룹 계정" 
            };
            if (!(await roleManager.RoleExistsAsync(guests.Name)))
            {
                await roleManager.CreateAsync(guests);
            }

            //[2] Users
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            //[2][1] Administrator
            // ('Administrator', '관리자', 'User', '응용 프로그램을 총 관리하는 사용자 계정')
            ApplicationUser administrator = await userManager.FindByEmailAsync($"administrator@{domainName}");
            if (administrator == null)
            {
                administrator = new ApplicationUser()
                {
                    UserName = $"administrator@{domainName}",
                    Email = $"administrator@{domainName}",
                    EmailConfirmed = true, // 기본 템플릿에는 이메일 인증 과정을 거치기때문에 EmailConfirmed 속성을 true로 설정해야 함.
                };
                await userManager.CreateAsync(administrator, "Pa$$w0rd");
            }

            //[2][2] Guest
            // ('Guest', '게스트 사용자', 'User', '게스트 사용자 계정')
            ApplicationUser guest = await userManager.FindByEmailAsync($"guest@{domainName}");
            if (guest == null)
            {
                guest = new ApplicationUser()
                {
                    UserName = "Guest",
                    Email = $"guest@{domainName}",
                    EmailConfirmed = false,
                };
                await userManager.CreateAsync(guest, "Pa$$w0rd");
            }

            //[2][3] Anonymous
            // ('Anonymous', '익명 사용자', 'User', '익명 사용자 계정')
            ApplicationUser anonymous = await userManager.FindByEmailAsync($"anonymous@{domainName}");
            if (anonymous == null)
            {
                anonymous = new ApplicationUser()
                {
                    UserName = "Anonymous",
                    Email = $"anonymous@{domainName}",
                    EmailConfirmed = false,
                };
                await userManager.CreateAsync(anonymous, "Pa$$w0rd");
            }

            //[3] UsersInRoles: AspNetUserRoles Table
            await userManager.AddToRoleAsync(administrator, Dul.Roles.Administrators.ToString());
            await userManager.AddToRoleAsync(administrator, Dul.Roles.Users.ToString());
            await userManager.AddToRoleAsync(guest, Dul.Roles.Guests.ToString());
            await userManager.AddToRoleAsync(anonymous, Dul.Roles.Guests.ToString());
        }
    } 
    #endregion
}

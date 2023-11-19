using Microsoft.AspNetCore.Identity;
using Hawaso.Models; // ApplicationUser, ApplicationRole 등의 모델을 사용하기 위해 필요
using System.Threading.Tasks;
using System;
using Hawaso.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Hawaso.Infrastructures
{
    public class UserAndRoleInitializer
    {
        #region Create BuiltIn Users and Roles 
        /// <summary>
        /// 내장 사용자 및 그룹(역할) 생성
        /// </summary>
        public static async Task CreateBuiltInUsersAndRoles(IServiceProvider serviceProvider)
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
}

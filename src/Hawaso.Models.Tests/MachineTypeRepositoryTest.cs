using MachineTypeApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MachineTypeApp.Models.Tests
{
    /// <summary>
    /// [!] Test Class: (Arrange -> Act -> Assert) Pattern
    /// 필요한 NuGet 패키지: Install-Package Microsoft.EntityFrameworkCore.InMemory
    /// </summary>
    [TestClass]
    public class MachineTypeRepositoryTest
    {
        [TestMethod]
        public async Task MachineTypeRepositoryAllMethodTest()
        {
            //[0] DbContextOptions<T> Object Creation
            var options = new DbContextOptionsBuilder<MachineTypeDbContext>()
                .UseInMemoryDatabase(databaseName: $"MachineTypeApp{Guid.NewGuid()}").Options;
            //.UseSqlServer("server=.;database=MachineTypeApp;integrated security=true;").Options;

            //[1] AddAsync() Method Test
            //[1][1] Repository 클래스를 사용하여 저장
            using (var context = new MachineTypeDbContext(options))
            {
                // Repository Object Creation
                //[!] Arrange
                var repository = new MachineTypeRepository(context);
                var model = new MachineType { Name = "[1] Machine Type" };

                //[!] Act: AddAsync() 메서드 테스트
                await repository.AddMachineTypeAsync(model);
                await context.SaveChangesAsync(); // 이 코드는 생략 가능
            }
            //[1][2] DbContext 클래스를 통해서 개수 및 레코드 확인 
            using (var context = new MachineTypeDbContext(options))
            {
                //[!] Assert
                Assert.AreEqual(1, await context.MachineTypes.CountAsync()); // true

                var model = await context.MachineTypes.Where(m => m.Id == 1).SingleOrDefaultAsync();
                Assert.AreEqual("[1] Machine Type", model?.Name); // true
            }

            //[2] GetAllAsync() Method Test
            using (var context = new MachineTypeDbContext(options))
            {
                // 트랜잭션 관련 코드는 InMemoryDatabase 공급자에서는 지원 X
                // using (var transaction = context.Database.BeginTransaction()) { transaction.Commit(); }

                var repository = new MachineTypeRepository(context);
                var model = new MachineType { Name = "[2] Machine Type" };
                await context.MachineTypes.AddAsync(model);
                await context.SaveChangesAsync(); //[1]
                await context.MachineTypes.AddAsync(new MachineType { Name = "[3] Machine Type" });
                await context.SaveChangesAsync(); //[2]
            }
            using (var context = new MachineTypeDbContext(options))
            {
                var repository = new MachineTypeRepository(context);
                var models = await repository.GetMachineTypesAsync();
                Assert.AreEqual(3, models.Count); // true
            }

            //[3] GetByIdAsync() Method Test
            using (var context = new MachineTypeDbContext(options))
            {
                // Empty
            }
            using (var context = new MachineTypeDbContext(options))
            {
                var repository = new MachineTypeRepository(context);
                var model = await repository.GetMachineTypeAsync(2);
                Assert.IsTrue(model.Name.Contains("Machine Type"));
                Assert.AreEqual("[2] Machine Type", model.Name);
            }

            //[4] EditAsync() Method Test
            using (var context = new MachineTypeDbContext(options))
            {
                // Empty
            }
            using (var context = new MachineTypeDbContext(options))
            {
                var repository = new MachineTypeRepository(context);
                var model = await repository.GetMachineTypeAsync(2);
                model.Name = "[2] Machine Type (Update)";
                await repository.EditMachineTypeAsync(model);
                await context.SaveChangesAsync(); // 생략가능 - 저장 시점을 코드로 표현하기 위함

                Assert.AreEqual("[2] Machine Type (Update)",
                    (await context.MachineTypes.Where(m => m.Id == 2).SingleOrDefaultAsync()).Name);
            }

            //[5] DeleteAsync() Method Test
            using (var context = new MachineTypeDbContext(options))
            {
                // Empty
            }
            using (var context = new MachineTypeDbContext(options))
            {
                var repository = new MachineTypeRepository(context);
                await repository.DeleteMachineTypeAsync(2);
                await context.SaveChangesAsync();

                Assert.AreEqual(2, await context.MachineTypes.CountAsync()); // true
                Assert.IsNull(await repository.GetMachineTypeAsync(2));
            }

            //[6] PagingAsync() Method Test
            using (var context = new MachineTypeDbContext(options))
            {
                // Empty
            }
            using (var context = new MachineTypeDbContext(options))
            {
                int pageIndex = 0;
                int pageSize = 1;

                var repository = new MachineTypeRepository(context);
                var models = await repository.GetAllAsync(pageIndex, pageSize);

                //Assert.AreEqual("[3] Machine Type", models.Items.FirstOrDefault().Name); // false
                Assert.AreEqual("[1] Machine Type", models.Items.FirstOrDefault().Name); // true
                Assert.AreEqual(2, models.TotalCount); // true
            }
        }
    }
}

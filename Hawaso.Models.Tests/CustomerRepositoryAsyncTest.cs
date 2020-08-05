using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetSaleCore.Models.Tests
{
    [TestClass]
    public class CustomerRepositoryAsyncTest
    {
        [TestMethod]
        public async Task CustomerRepositoryAsyncAllMethodTest()
        {
            // DbContextOptions<T> Object Creation
            var options = new DbContextOptionsBuilder<DotNetSaleCoreDbContext>()
                .UseInMemoryDatabase(databaseName: $"DotNetSaleCore{Guid.NewGuid()}").Options;
            //.UseSqlServer("server=(localdb)\\mssqllocaldb;database=DotNetSaleCore;integrated security=true;").Options;

            // ILoggerFactory Object Creation
            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>();

            //[1] AddAsync() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                // Repository Object Creation
                //[!] Arrange
                var repository = new CustomerRepository(context, factory);
                var model = new Customer { CustomerName = "[1] 고객이름", Created = DateTime.Now };

                //[!] Act
                await repository.AddAsync(model);
                await context.SaveChangesAsync();
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                //[!] Assert
                Assert.AreEqual(1, await context.Customers.CountAsync());
                var model = await context.Customers.Where(m => m.CustomerId == 1).SingleOrDefaultAsync();
                Assert.AreEqual("[1] 고객이름", model?.CustomerName);
            }

            //[2] GetAllAsync() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                //// 트랜잭션 관련 코드는 InMemoryDatabase 공급자에서는 지원 X
                ////using (var transaction = context.Database.BeginTransaction()) { transaction.Commit(); }
                var repository = new CustomerRepository(context, factory);
                var model = new Customer { CustomerName = "[2] 홍길동", Created = DateTime.Now };
                await context.Customers.AddAsync(model);
                await context.SaveChangesAsync(); //[1]
                await context.Customers.AddAsync(new Customer { CustomerName = "[3] 백두산", Created = DateTime.Now });
                await context.SaveChangesAsync(); //[2]
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                var repository = new CustomerRepository(context, factory);
                var models = await repository.GetAllAsync();
                Assert.AreEqual(3, models.Count);
            }

            //[3] GetByIdAsync() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                // Empty
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                var repository = new CustomerRepository(context, factory);
                var model = await repository.GetByIdAsync(2);
                Assert.IsTrue(model.CustomerName.Contains("길동"));
                Assert.AreEqual("[2] 홍길동", model.CustomerName);
            }

            //[4] GetEditAsync() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                // Empty
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                var repository = new CustomerRepository(context, factory);
                var model = await repository.GetByIdAsync(2);
                model.CustomerName = "[2] 임꺽정";
                await repository.EditAsync(model);
                await context.SaveChangesAsync();

                Assert.AreEqual("[2] 임꺽정",
                    (await context.Customers.Where(m => m.CustomerId == 2).SingleOrDefaultAsync()).CustomerName);
            }

            //[5] GetDeleteAsync() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                // Empty
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                var repository = new CustomerRepository(context, factory);
                await repository.DeleteAsync(2);
                await context.SaveChangesAsync();

                Assert.AreEqual(2, await context.Customers.CountAsync());
                Assert.IsNull(await repository.GetByIdAsync(2));
            }

            //[6] PagingAsync() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                // Empty
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                int pageIndex = 0;
                int pageSize = 1;

                var repository = new CustomerRepository(context, factory);
                var models = await repository.GetAllAsync(pageIndex, pageSize);

                Assert.AreEqual("[3] 백두산", models.Records.FirstOrDefault().CustomerName);
                Assert.AreEqual(2, models.TotalRecords);
            }
        }
    }
}

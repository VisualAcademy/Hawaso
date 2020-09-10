using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetSaleCore.Models.Tests
{
    /// <summary>
    /// [7] Test Class
    /// </summary>
    [TestClass]
    public class ProductRepositoryAsyncTest
    {
        [TestMethod]
        public async Task ProductRepositoryAsyncAllMethodTest()
        {
            #region [0] DbContextOptions<T> Object Creation and ILoggerFactory Object Creation
            //[0] DbContextOptions<T> Object Creation and ILoggerFactory Object Creation
            var options = new DbContextOptionsBuilder<DotNetSaleCoreDbContext>()
                .UseInMemoryDatabase(databaseName: $"DotNetSaleCore{Guid.NewGuid()}").Options;
            //.UseSqlServer("server=(localdb)\\mssqllocaldb;database=DotNetSaleCore;integrated security=true;").Options;

            // ILoggerFactory Object Creation
            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>(); 
            #endregion

            #region [1] AddAsync() Method Test
            //[1] AddAsync() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                // Repository Object Creation
                //[!] Arrange
                var repository = new ProductRepositoryAsync(context, factory);
                var model = new Product { ModelName = "[1] 제품이름", RegistDate = DateTime.Now };

                //[!] Act
                await repository.AddAsync(model);
                await context.SaveChangesAsync();
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                //[!] Assert
                Assert.AreEqual(1, await context.Products.CountAsync());
                var model = await context.Products.Where(m => m.ProductId == 1).SingleOrDefaultAsync();
                Assert.AreEqual("[1] 제품이름", model?.ModelName);
            } 
            #endregion

            #region [2] GetAllAsync() Method Test
            //[2] GetAllAsync() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                // 트랜잭션 관련 코드는 InMemoryDatabase 공급자에서는 지원 X
                //using (var transaction = context.Database.BeginTransaction()) { transaction.Commit(); }
                var repository = new ProductRepositoryAsync(context, factory);
                var model = new Product { ModelName = "[2] 홍길동", RegistDate = DateTime.Now };
                await context.Products.AddAsync(model);
                await context.SaveChangesAsync(); //[1]
                await context.Products.AddAsync(new Product { ModelName = "[3] 백두산", RegistDate = DateTime.Now });
                await context.SaveChangesAsync(); //[2]
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                var repository = new ProductRepositoryAsync(context, factory);
                var models = await repository.GetAllAsync();
                Assert.AreEqual(3, models.Count);
            } 
            #endregion

            #region [3] GetByIdAsync() Method Test
            //[3] GetByIdAsync() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                // Empty
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                var repository = new ProductRepositoryAsync(context, factory);
                var model = await repository.GetByIdAsync(2);
                Assert.IsTrue(model.ModelName.Contains("길동"));
                Assert.AreEqual("[2] 홍길동", model.ModelName);
            }
            #endregion

            #region [4] EditAsync() Method Test
            //[4] EditAsync() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                // Empty
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                var repository = new ProductRepositoryAsync(context, factory);
                var model = await repository.GetByIdAsync(2);
                model.ModelName = "[2] 임꺽정";
                await repository.EditAsync(model);
                await context.SaveChangesAsync();

                Assert.AreEqual("[2] 임꺽정",
                    (await context.Products.Where(m => m.ProductId == 2).SingleOrDefaultAsync()).ModelName);
            }
            #endregion

            #region [5] DeleteAsync() Method Test
            //[5] DeleteAsync() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                // Empty
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                var repository = new ProductRepositoryAsync(context, factory);
                await repository.DeleteAsync(2);
                await context.SaveChangesAsync();

                Assert.AreEqual(2, await context.Products.CountAsync());
                Assert.IsNull(await repository.GetByIdAsync(2));
            }
            #endregion

            #region [6] GetAllAsync(PagingAsync)() Method Test
            //[6] GetAllAsync(PagingAsync)() Method Test
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                // Empty
            }
            using (var context = new DotNetSaleCoreDbContext(options))
            {
                int pageIndex = 0;
                int pageSize = 1;

                var repository = new ProductRepositoryAsync(context, factory);
                var models = await repository.GetAllAsync(pageIndex, pageSize);

                Assert.AreEqual("[3] 백두산", models.Records.FirstOrDefault().ModelName);
                Assert.AreEqual(2, models.TotalRecords);
            } 
            #endregion
        }
    }
}

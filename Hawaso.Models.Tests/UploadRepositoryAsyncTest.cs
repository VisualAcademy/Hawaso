using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace UploadApp.Models.Tests
{
    /// <summary>
    /// [7] Test Class
    /// </summary>
    [TestClass]
    public class UploadRepositoryAsyncTest
    {
        [TestMethod]
        public async Task UploadRepositoryAsyncAllMethodTest()
        {
            #region [0] DbContextOptions<T> Object Creation and ILoggerFactory Object Creation
            //[0] DbContextOptions<T> Object Creation and ILoggerFactory Object Creation
            var options = new DbContextOptionsBuilder<UploadAppDbContext>()
                .UseInMemoryDatabase(databaseName: $"UploadApp{Guid.NewGuid()}").Options;
            //.UseSqlServer("server=(localdb)\\mssqllocaldb;database=UploadApp;integrated security=true;").Options;

            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>();
            #endregion

            #region [1] AddAsync() Method Test
            //[1] AddAsync() Method Test
            using (var context = new UploadAppDbContext(options))
            {
                context.Database.EnsureCreated(); // 데이터베이스가 만들어져 있는지 확인

                //[A] Arrange
                var repository = new UploadRepository(context, factory);
                var model = new Upload { Name = "[1] 관리자", Title = "공지사항입니다.", Content = "내용입니다.", ParentId = 1, ParentKey = "1" };

                //[B] Act
                await repository.AddAsync(model); // Id: 1
            }
            using (var context = new UploadAppDbContext(options))
            {
                //[C] Assert
                Assert.AreEqual(1, await context.Uploads.CountAsync());
                var model = await context.Uploads.Where(n => n.Id == 1).SingleOrDefaultAsync();
                Assert.AreEqual("[1] 관리자", model.Name);
            }
            #endregion

            #region [2] GetAllAsync() Method Test
            //[2] GetAllAsync() Method Test
            using (var context = new UploadAppDbContext(options))
            {
                // 트랜잭션 관련 코드는 InMemoryDatabase 공급자에서는 지원 X
                //using (var transaction = context.Database.BeginTransaction()) { transaction.Commit(); }
                //[A] Arrange
                var repository = new UploadRepository(context, factory);
                var model = new Upload { Name = "[2] 홍길동", Title = "공지사항입니다.", Content = "내용입니다." };

                //[B] Act
                await repository.AddAsync(model); // Id: 2
                await repository.AddAsync(new Upload { Name = "[3] 백두산", Title = "자유게시판입니다.", ParentId = 3, ParentKey = "1" }); // Id: 3
            }
            using (var context = new UploadAppDbContext(options))
            {
                //[C] Assert
                var repository = new UploadRepository(context, factory);
                var models = await repository.GetAllAsync();
                Assert.AreEqual(3, models.Count()); // TotalRecords: 3
            }
            #endregion

            #region [3] GetByIdAsync() Method Test
            //[3] GetByIdAsync() Method Test
            using (var context = new UploadAppDbContext(options))
            {
                // Empty
            }
            using (var context = new UploadAppDbContext(options))
            {
                var repository = new UploadRepository(context, factory);
                var model = await repository.GetByIdAsync(2);
                Assert.IsTrue(model.Name.Contains("길동"));
                Assert.AreEqual("[2] 홍길동", model.Name);
            }
            #endregion

            #region [4] EditAsync() Method Test
            //[4] EditAsync() Method Test
            using (var context = new UploadAppDbContext(options))
            {
                // Empty
            }
            using (var context = new UploadAppDbContext(options))
            {
                var repository = new UploadRepository(context, factory);
                var model = await repository.GetByIdAsync(2);

                model.Name = "[2] 임꺽정"; // Modified
                await repository.EditAsync(model);

                var updateModel = await repository.GetByIdAsync(2);

                Assert.IsTrue(updateModel.Name.Contains("꺽정"));
                Assert.AreEqual("[2] 임꺽정", updateModel.Name);
                Assert.AreEqual("[2] 임꺽정",
                    (await context.Uploads.Where(m => m.Id == 2).SingleOrDefaultAsync())?.Name);
            }
            #endregion

            #region [5] DeleteAsync() Method Test
            //[5] DeleteAsync() Method Test
            using (var context = new UploadAppDbContext(options))
            {
                // Empty
            }
            using (var context = new UploadAppDbContext(options))
            {
                var repository = new UploadRepository(context, factory);
                await repository.DeleteAsync(2);

                Assert.AreEqual(2, (await context.Uploads.CountAsync()));
                Assert.IsNull(await repository.GetByIdAsync(2));
            }
            #endregion

            #region [6] GetAllAsync(PagingAsync)() Method Test
            //[6] GetAllAsync(PagingAsync)() Method Test
            using (var context = new UploadAppDbContext(options))
            {
                // Empty
            }
            using (var context = new UploadAppDbContext(options))
            {
                int pageIndex = 0;
                int pageSize = 1;

                var repository = new UploadRepository(context, factory);
                var noticesSet = await repository.GetAllAsync(pageIndex, pageSize);

                var firstName = noticesSet.Records.FirstOrDefault()?.Name;
                var recordCount = noticesSet.TotalRecords;

                Assert.AreEqual("[3] 백두산", firstName);
                Assert.AreEqual(2, recordCount);
            }
            #endregion

            #region [7] GetStatus() Method Test
            //[7] GetStatus() Method Test
            using (var context = new UploadAppDbContext(options))
            {
                int parentId = 1;

                var no1 = await context.Uploads.Where(m => m.Id == 1).SingleOrDefaultAsync();
                no1.ParentId = parentId;
                no1.IsPinned = true; // Pinned

                context.Entry(no1).State = EntityState.Modified;
                context.SaveChanges();

                var repository = new UploadRepository(context, factory);
                var r = await repository.GetStatus(parentId);

                Assert.AreEqual(1, r.Item1); // Pinned Count == 1
            }
            #endregion

            //[9] GetArticles() Method Test
            using (var context = new UploadAppDbContext(options))
            {
                var repository = new UploadRepository(context, factory);
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "", "", 0); // [3] 백두산, [1] 관리자
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "두", "", 0); // [3] 백두산
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "", "Name", 0); // [1] 관리자, [3] 백두산
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "", "TitleDesc", 0); // 자유게시판, 공지사항
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "", "Title", 0); // 공지사항, 자유게시판
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "", "TitleDesc", 1); // 공지사항
                var articleSet = await repository.GetArticles<string>(0, 10, "", "", "TitleDesc", "1"); // 자, 공
                foreach (var item in articleSet.Items)
                {
                    Console.WriteLine($"{item.Name} - {item.Title}");
                }
            }
        }
    }
}

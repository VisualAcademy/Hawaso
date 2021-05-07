using Hawaso.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MemoApp.Models.Tests
{
    /// <summary>
    /// [!] Test Class: (Arrange -> Act -> Assert) Pattern
    /// 필요한 NuGet 패키지: Install-Package Microsoft.EntityFrameworkCore.InMemory
    /// </summary>
    [TestClass]
    public class MemoRepositoryTest
    {
        [TestMethod]
        public async Task MemoRepositoryAllMethodTest()
        {
            #region [0] DbContextOptions<T> Object Creation and ILoggerFactory Object Creation
            //[0] DbContextOptions<T> Object Creation and ILoggerFactory Object Creation
            var options = new DbContextOptionsBuilder<MemoAppDbContext>()
                .UseInMemoryDatabase(databaseName: $"MemoApp{Guid.NewGuid()}").Options;
                //.UseSqlServer("server=(localdb)\\mssqllocaldb;database=MemoApp;integrated security=true;").Options;

            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>();
            #endregion

            #region [1] AddAsync() Method Test
            //[1] AddAsync() Method Test
            //[1][1] Repository 클래스를 사용하여 저장
            using (var context = new MemoAppDbContext(options))
            {
                context.Database.EnsureCreated(); // 데이터베이스가 만들어져 있는지 확인

                //[A] Arrange: 1번 데이터를 아래 항목으로 저장합니다. 
                var repository = new MemoRepository(context, factory);
                var model = new Memo { Name = "[1] 관리자", Title = "Q&A입니다.", Content = "내용입니다.", ParentId = 1, ParentKey = "1" };

                //[B] Act: AddAsync() 메서드 테스트
                await repository.AddAsync(model); // Id: 1
            }
            //[1][2] DbContext 클래스를 통해서 개수 및 레코드 확인 
            using (var context = new MemoAppDbContext(options))
            {
                //[C] Assert: 현재 총 데이터 개수가 1개인 것과, 1번 데이터의 이름이 "[1] 관리자"인지 확인합니다. 
                Assert.AreEqual(1, await context.Memos.CountAsync());

                var model = await context.Memos.Where(n => n.Id == 1).SingleOrDefaultAsync();
                Assert.AreEqual("[1] 관리자", model.Name);
            }
            #endregion

            #region [2] GetAllAsync() Method Test
            //[2] GetAllAsync() Method Test
            using (var context = new MemoAppDbContext(options))
            {
                // 트랜잭션 관련 코드는 InMemoryDatabase 공급자에서는 지원 X
                // using (var transaction = context.Database.BeginTransaction()) { transaction.Commit(); }

                //[A] Arrange
                var repository = new MemoRepository(context, factory);
                var model = new Memo { Name = "[2] 홍길동", Title = "Q&A입니다.", Content = "내용입니다." };

                //[B] Act
                await repository.AddAsync(model); // Id: 2
                await repository.AddAsync(new Memo { Name = "[3] 백두산", Title = "Q&A입니다.", ParentId = 3, ParentKey = "1" }); // Id: 3
            }
            {
                using var db = new MemoAppDbContext(options);
                //[C] Assert
                var repository = new MemoRepository(db, factory);
                var models = await repository.GetAllAsync();
                Assert.AreEqual(3, models.Count()); // TotalRecords: 3
            }
            #endregion

            #region [3] GetByIdAsync() Method Test
            //[3] GetByIdAsync() Method Test
            using (var context = new MemoAppDbContext(options))
            {
                // Empty
            }
            using (var context = new MemoAppDbContext(options))
            {
                var repository = new MemoRepository(context, factory);
                var model = await repository.GetByIdAsync(2);
                Assert.IsTrue(model.Name.Contains("길동"));
                Assert.AreEqual("[2] 홍길동", model.Name);
            }
            #endregion

            #region [4] EditAsync() Method Test
            //[4] EditAsync() Method Test
            using (var context = new MemoAppDbContext(options))
            {
                // Empty
            }
            using (var context = new MemoAppDbContext(options))
            {
                var repository = new MemoRepository(context, factory);
                var model = await repository.GetByIdAsync(2);

                model.Name = "[2] 임꺽정"; // Modified
                await repository.EditAsync(model);

                var updateModel = await repository.GetByIdAsync(2);

                Assert.IsTrue(updateModel.Name.Contains("꺽정"));
                Assert.AreEqual("[2] 임꺽정", updateModel.Name);
                Assert.AreEqual("[2] 임꺽정",
                    (await context.Memos.Where(m => m.Id == 2).SingleOrDefaultAsync())?.Name);
            }
            #endregion

            #region [5] DeleteAsync() Method Test
            //[5] DeleteAsync() Method Test
            using (var context = new MemoAppDbContext(options))
            {
                // Empty
            }
            using (var context = new MemoAppDbContext(options))
            {
                var repository = new MemoRepository(context, factory);
                await repository.DeleteAsync(2);

                Assert.AreEqual(2, (await context.Memos.CountAsync()));
                Assert.IsNull(await repository.GetByIdAsync(2));
            }
            #endregion

            #region [6] GetAllAsync(PagingAsync)() Method Test
            //[6] GetAllAsync(PagingAsync)() Method Test
            using (var context = new MemoAppDbContext(options))
            {
                // Empty
            }
            using (var context = new MemoAppDbContext(options))
            {
                int pageIndex = 0;
                int pageSize = 1;

                var repository = new MemoRepository(context, factory);
                var noticesSet = await repository.GetAllAsync(pageIndex, pageSize);

                var firstName = noticesSet.Records.FirstOrDefault()?.Name;
                var recordCount = noticesSet.TotalRecords;

                Assert.AreEqual("[3] 백두산", firstName);
                Assert.AreEqual(2, recordCount);
            }
            #endregion

            #region [7] GetStatus() Method Test
            //[7] GetStatus() Method Test
            using (var context = new MemoAppDbContext(options))
            {
                int parentId = 1;

                var no1 = await context.Memos.Where(m => m.Id == 1).SingleOrDefaultAsync();
                no1.ParentId = parentId;
                no1.IsPinned = true; // Pinned

                context.Entry(no1).State = EntityState.Modified;
                context.SaveChanges();

                var repository = new MemoRepository(context, factory);
                var r = await repository.GetStatus(parentId);

                Assert.AreEqual(1, r.Item1); // Pinned Count == 1
            }
            #endregion

            #region [8] GetArticles() Method Test
            //[8] GetArticles() Method Test
            using (var context = new MemoAppDbContext(options))
            {
                var repository = new MemoRepository(context, factory);
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "", "", 0); // [3] 백두산, [1] 관리자
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "두", "", 0); // [3] 백두산
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "", "Name", 0); // [1] 관리자, [3] 백두산
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "", "TitleDesc", 0); // Q&A, Q&A
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "", "Title", 0); // Q&A, Q&A
                //var articleSet = await repository.GetArticles<int>(0, 10, "", "", "TitleDesc", 1); // Q&A
                var articleSet = await repository.GetArticlesAsync<string>(0, 10, "", "", "TitleDesc", "1"); // 자, 공
                foreach (var item in articleSet.Items)
                {
                    Console.WriteLine($"{item.Name} - {item.Title}");
                }
            }
            #endregion

            //#region [9] AddRange() Method Test
            ////[9] AddRange() Method Test
            //using (var context = new MemoAppDbContext(options))
            //{
            //    var repository = new MemoRepository(context, factory);
            //    var reply1 = new Memo { Name = "답변 1", };
            //    var reply2 = new Memo { Name = "답변 2", };
            //    var reply3 = new Memo { Name = "답변 3", };
            //    var reply4 = new Memo { Name = "답변 4", };

            //    context.Memos.AddRange(reply1, reply2, reply3, reply4);
            //    context.SaveChanges();

            //    var replys = await repository.GetArticles<int>(0, 10, "Name", "답변 ", "", 0);
            //    Assert.AreEqual(4, replys.Items.Count());
            //}
            //#endregion
        }
    }
}

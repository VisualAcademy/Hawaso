//using Dul.Data;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace DotNetSaleCore.Models.Tests
//{
//    [TestClass]
//    [TestCategory("CategoryBase 테스트")]
//    public class CategoryBaseRepositoryTest
//    {
//        private CategoryBaseRepository repository;

//        public CategoryBaseRepositoryTest()
//        {
//            var connectionString =
//                "server=(localdb)\\mssqllocaldb;" +
//                "database=DotNetSaleCore;integrated security=true;";
//            repository = new CategoryBaseRepository(connectionString);
//        }

//        /// <summary>
//        /// [00] 카테고리 컬렉션 출력 메서드
//        /// </summary>
//        private static void PrintCategories(List<CategoryBase> categories)
//        {
//            if (categories != null)
//            {
//                foreach (var category in categories)
//                {
//                    Console.WriteLine(
//                        $"{category.CategoryId} - {category.CategoryName}");
//                }
//            }
//        }

//        /// <summary>
//        /// [01] Open() 메서드 테스트 
//        /// </summary>
//        [TestMethod]
//        [TestCategory("[01] Open() 메서드 테스트")]
//        public void OpenMethodTest()
//        {
//            var result = repository.Open();

//            if (result)
//            {
//                Console.WriteLine("데이터베이스 연결 성공");
//            }
//            else
//            {
//                Console.WriteLine("데이터베이스 연결 실패");
//            }
//        }

//        /// <summary>
//        /// [02] Log() 메서드 테스트
//        /// </summary>
//        [TestMethod]
//        public void LogMethodTest()
//        {
//            string message = "로그 메서드 테스트";

//            repository.Log(message);
//        }

//        /// <summary>
//        /// [03] Has() 메서드 테스트
//        /// </summary>
//        [TestMethod]
//        public void HasMethodTest()
//        {
//            int count = repository.Has();

//            Console.WriteLine($"{count}개의 데이터가 있습니다.");
//        }

//        /// <summary>
//        /// [04] Add() 메서드 테스트 
//        /// </summary>
//        [TestMethod]
//        public void AddMethodTest()
//        {
//            CategoryBase model = new CategoryBase();
//            model.CategoryName = "생활용품";

//            var r = repository.Add(model);

//            Assert.AreEqual(r.CategoryName, model.CategoryName);
//        }

//        /// <summary>
//        /// [05] Read() 메서드 테스트 
//        /// </summary>
//        [TestMethod]
//        public void ReadMethodTest()
//        {
//            var categories = repository.Read();

//            PrintCategories(categories);
//        }

//        /// <summary>
//        /// [06] Browse() 메서드 테스트 
//        /// </summary>
//        [TestMethod]
//        public void BrowseMethodTest()
//        {
//            int categoryId = 1;
//            var category = repository.Browse(categoryId);
//            if (category != null)
//            {
//                Console.WriteLine(
//                    $"{category.CategoryId} - {category.CategoryName}");
//            }
//            else
//            {
//                Console.WriteLine($"{categoryId}번 카테고리가 없습니다.");
//            }
//        }

//        /// <summary>
//        /// [07] Edit() 메서드 테스트 
//        /// </summary>
//        [TestMethod]
//        public void EditMethodTest()
//        {
//            var model = new CategoryBase { CategoryId = 0, CategoryName = "BOOKS" };

//            var isEdited = repository.Edit(model);

//            if (isEdited)
//            {
//                Console.WriteLine("수정했습니다.");
//            }
//            else
//            {
//                Console.WriteLine("수정하지 못했습니다.");
//            }
//        }

//        /// <summary>
//        /// [08] Delete() 메서드 테스트 
//        /// </summary>
//        [TestMethod]
//        public void DeleteMethodTest()
//        {
//            var number = 3;

//            var isDeleted = repository.Delete(number);

//            if (isDeleted)
//            {
//                Console.WriteLine("삭제했습니다.");
//            }
//            else
//            {
//                Console.WriteLine("삭제하지 못했습니다.");
//            }
//        }

//        /// <summary>
//        /// [09] Search() 메서드 테스트 
//        /// </summary>
//        [TestMethod]
//        public void SearchMethodTest()
//        {
//            var categories = repository.Search("용");

//            PrintCategories(categories);
//        }

//        /// <summary>
//        /// [10] Paging() 메서드 테스트 
//        /// </summary>
//        [TestMethod]
//        public void PagingMethodTest()
//        {
//            var categories = repository.Paging(2, 3);

//            PrintCategories(categories);
//        }

//        /// <summary>
//        /// [11] Ordering() 메서드 테스트
//        /// </summary>
//        [TestMethod]
//        public void OrderingMethodTest()
//        {
//            var categories = repository.Ordering(OrderOption.None);

//            PrintCategories(categories.ToList());
//        }
//    }
//}

using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Hawaso.Models
{
    public class LoginRepository : ILoginRepository
    {
        private readonly SqlConnection db;

        public LoginRepository(string connectionString)
        {
            db = new SqlConnection(connectionString);
        }

        public void Add(Login model)
        {
            var sql = "Insert Into Logins (UserName, LoginIp) Values (@UserName, @LoginIp); ";
            db.Execute(sql, model);
        }

        /// <summary>
        /// 리스트
        /// GetAll(), FindAll() 형태로 주로 사용
        /// 또는
        /// GetNotes(), GetComments(), GetLogins() 형태로도 많이 사용
        /// </summary>
        /// <param name="pageNumber">페이지 번호</param>
        /// <param name="pageSize">한 페이지에 표시할 레코드 수</param>
        public List<Login> GetAll(int pageNumber, int pageSize = 10)
        {
            try
            {
                var parameters = new DynamicParameters(new { PageNumber = pageNumber, PageSize = pageSize });
                return db.Query<Login>("LoginsList", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }

        public List<Login> GetAll(string userName, int pageNumber, int pageSize = 10)
        {
            try
            {
                var parameters = new DynamicParameters(new { PageNumber = pageNumber, PageSize = pageSize, UserName = userName });
                return db.Query<Login>("LoginsList", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }

        /// <summary>
        /// 관리자 전용 로그인 리스트
        /// </summary>
        public List<Login> GetAllAdmin(int pageNumber, int pageSize = 10)
        {
            try
            {
                var parameters = new DynamicParameters(new { PageNumber = pageNumber, PageSize = pageSize });
                return db.Query<Login>("LoginsListAdmin", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }

        /// <summary>
        /// 전체 레코드 수 반환: Logins 테이블의 모든 레코드 수 
        /// </summary>
        public int GetCountAll()
        {
            try
            {
                return db.Query<int>("Select Count(*) From Logins").SingleOrDefault();
            }
            catch (System.Exception)
            {
                return -1;
            }
        }

        public int GetCountAll(string userName)
        {
            try
            {
                return db.Query<int>(
                    "Select Count(*) From Logins Where UserName = @UserName", new { UserName = userName }).SingleOrDefault();
            }
            catch (System.Exception)
            {
                return -1;
            }
        }

        public int GetCountAllAdmin()
        {
            try
            {
                return db.Query<int>("Select Count(*) From Logins").SingleOrDefault();
            }
            catch (System.Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// 검색 결과에 따른 총 레코드 카운트
        /// </summary>
        public int GetCountBySearch(string searchField, string searchQuery)
        {
            try
            {
                return db.Query<int>("LoginsSearchCount", new
                {
                    SearchField = searchField,
                    SearchQuery = searchQuery
                },
                    commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();

            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }

        /// <summary>
        /// 검색 결과에 따른 총 레코드 카운트: 회원 전용 
        /// </summary>
        public int GetCountBySearch(string userName, string searchField, string searchQuery)
        {
            try
            {
                return db.Query<int>("LoginsSearchCount", new
                {
                    SearchField = searchField,
                    SearchQuery = searchQuery,
                    UserName = userName
                },
                    commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();

            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }

        public int GetCountBySearchAdmin(string searchField, string searchQuery, DateTime startDate, DateTime endDate)
        {
            try
            {
                return db.Query<int>("LoginsSearchCountAdmin", new
                {
                    SearchField = searchField,
                    SearchQuery = searchQuery,
                    StartDate = startDate,
                    EndDate = endDate
                },
                    commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();

            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }

        /// <summary>
        /// 검색 결과 리스트
        /// </summary>
        public List<Login> GetSearchAll(string searchField, string searchQuery, int pageNumber, int pageSize = 10)
        {
            var parameters = new DynamicParameters(
                new { SearchField = searchField, SearchQuery = searchQuery, PageNumber = pageNumber, PageSize = pageSize });
            return db.Query<Login>("LoginsSearchList", parameters, commandType: CommandType.StoredProcedure).ToList();
        }

        /// <summary>
        /// 검색 결과 리스트: 회원 전용
        /// </summary>
        public List<Login> GetSearchAll(string userName, string searchField, string searchQuery, int pageNumber, int pageSize = 10)
        {
            var parameters = new DynamicParameters(
                new { SearchField = searchField, SearchQuery = searchQuery, PageNumber = pageNumber, PageSize = pageSize, UserName = userName });
            return db.Query<Login>("LoginsSearchList", parameters, commandType: CommandType.StoredProcedure).ToList();
        }

        public List<Login> GetSearchAllAdmin(string searchField, string searchQuery, DateTime startDate, DateTime endDate, int pageNumber, int pageSize = 10)
        {
            var parameters = new DynamicParameters(
                new { SearchField = searchField, SearchQuery = searchQuery, StartDate = startDate, EndDate = endDate, PageNumber = pageNumber, PageSize = pageSize });
            return db.Query<Login>("LoginsSearchListAdmin", parameters, commandType: CommandType.StoredProcedure).ToList();
        }
    }
}

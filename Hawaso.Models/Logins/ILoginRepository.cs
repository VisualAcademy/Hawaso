using System;
using System.Collections.Generic;

namespace Hawaso.Models
{
    public interface ILoginRepository
    {
        void Add(Login model); 

        /// <summary>
        /// 전체 레코드 수 반환: Answers 테이블의 모든 레코드 수 
        /// </summary>
        int GetCountAll();
        int GetCountAll(string userName);
        List<Login> GetAllAdmin(int pageNumber, int pageSize = 10);

        /// <summary>
        /// 검색 결과에 따른 총 레코드 카운트
        /// </summary>
        int GetCountBySearch(string searchField, string searchQuery);
        int GetCountBySearch(string userName, string searchField, string searchQuery);

        /// <summary>
        /// 게시판 리스트
        /// GetAll(), FindAll() 형태로 주로 사용
        /// 또는
        /// GetNotes(), GetComments(), GetAnswers() 형태로도 많이 사용
        /// </summary>
        /// <param name="pageNumber">페이지 번호</param>
        /// <param name="pageSize">한 페이지에 표시할 레코드 수</param>
        List<Login> GetAll(int pageNumber, int pageSize = 10);
        List<Login> GetAll(string userName, int pageNumber, int pageSize = 10);

        /// <summary>
        /// 검색 결과 리스트 
        /// </summary>
        List<Login> GetSearchAll(string searchField, string searchQuery, int pageNumber, int pageSize = 10);
        List<Login> GetSearchAll(string userName, string searchField, string searchQuery, int pageNumber, int pageSize = 10);

        int GetCountBySearchAdmin(string searchField, string searchQuery, DateTime startDate, DateTime endDate);
        List<Login> GetSearchAllAdmin(string searchField, string searchQuery, DateTime startDate, DateTime endDate, int pageNumber, int pageSize = 10);
        int GetCountAllAdmin();
    }
}

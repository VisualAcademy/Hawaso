using System.Collections.Generic;

namespace MachineTypeApp.Models
{
    /// <summary>
    /// 페이징된 아티클과 아티클 개수
    /// </summary>
    /// <typeparam name="T">모델 클래스</typeparam>
    /// <typeparam name="V">개수 형식(int, long)</typeparam>
    public struct ArticleSet<T, V>
    {
        /// <summary>
        /// 아티클 리스트: 현재 페이지에 해당하는 아티클 리스트 
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// 아티클 수: 현재 앱의 지정된 컬렉션의 레코드 수
        /// </summary>
        public V TotalCount { get; set; }

        public ArticleSet(IEnumerable<T> items, V totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }
    }
}

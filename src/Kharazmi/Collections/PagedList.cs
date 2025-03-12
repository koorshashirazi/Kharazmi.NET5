#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#endregion

namespace Kharazmi.Collections
{
    public interface IPagedList<T> : IList<T>
        where T : class
    {
        int TotalCount { get; set; }
        int TotalPages { get; set; }
        int PageIndex { get; set; }
        int PageSize { get; set; }
        bool HasPrevious { get; }
        bool HasNext { get; }
    }

    public class PagedList<T> : List<T>, IPagedList<T>
        where T : class
    {
        public PagedList()
        {
        }

        public PagedList([MaybeNull] IEnumerable<T> source, int pageIndex, int pageSize, int? totalCount)
        {
            BuildPaging(source, pageIndex, pageSize, totalCount);
        }


        public static IPagedList<T> Empty => new PagedList<T>();

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => PageIndex > 0;
        public bool HasNext => PageIndex + 1 < TotalCount;

        private void BuildPaging(IEnumerable<T> source, int pageIndex, int pageSize, int? totalCount)
        {
            var list = source.ToList();

            PageIndex = pageIndex < 0 ? PageIndex : pageIndex;
            PageSize = pageSize > 0 ? pageSize : PageSize;
            TotalCount = totalCount ?? list.Count;

            if (pageSize > 0)
            {
                TotalPages = TotalCount / pageSize;
                if (TotalCount % pageSize > 0)
                    TotalPages++;
            }

            PageSize = pageSize;
            PageIndex = pageIndex;
            source = totalCount is null ? list.Skip(pageIndex * pageSize).Take(pageSize) : list;
            AddRange(source);
        }
    }
}
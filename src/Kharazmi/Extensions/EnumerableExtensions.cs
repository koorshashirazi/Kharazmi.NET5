#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kharazmi.Collections;
using Kharazmi.Guard;

#endregion

namespace Kharazmi.Extensions
{
    public static class EnumerableExtensions
    {
        private const int DefaultPageIndex = 0;
        private const int DefaultPageSize = 10;

        private static class DefaultReadOnlyCollection<T>
        {
            private static ReadOnlyCollection<T>? _defaultCollection;
            internal static ReadOnlyCollection<T> Empty => _defaultCollection ??= new ReadOnlyCollection<T>(new T[0]);
        }

        public static ReadOnlyCollection<T> AsReadOnly<T>([NotNull] this IEnumerable<T> source)
        {
            var enumerable = source as T[] ?? source.ToArray();
            if (!enumerable.Any())
                return DefaultReadOnlyCollection<T>.Empty;

            return source switch
            {
                ReadOnlyCollection<T> readOnly => readOnly,
                List<T> list => list.AsReadOnly(),
                _ => new ReadOnlyCollection<T>(enumerable.ToArray())
            };
        }

        public static IPagedList<T> PageBy<T>([NotNull] this IEnumerable<T> source, int pageIndex,
            int pageSize, int? totalCount = null) where T : class
        {
            source = source.NotNull(nameof(source)).ToList();

            if (pageIndex < 0)
                pageIndex = DefaultPageIndex;

            if (pageSize <= 0)
                pageSize = DefaultPageSize;

            var pageResult = new PagedList<T> {TotalCount = totalCount ?? source.Count()};

            source = totalCount is null ? source.Skip(pageIndex * pageSize).Take(pageSize) : source;
            pageResult.AddRange(source);

            pageResult.TotalPages = pageResult.TotalCount / pageSize;
            if (pageResult.TotalCount % pageSize > 0)
                pageResult.TotalPages++;

            pageResult.PageSize = pageSize;
            pageResult.PageIndex = pageIndex;

            return pageResult;
        }
    }
}
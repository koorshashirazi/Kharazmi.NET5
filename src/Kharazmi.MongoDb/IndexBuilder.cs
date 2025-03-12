using System;
using System.Linq.Expressions;

namespace Kharazmi.Localization
{
    public class IndexBuilder<TDocument>
    {
        public IndexBuilder(
            Expression<Func<TDocument, object>> expression,
            bool isAscending = true)
        {
            Expression = expression;
            IsAscending = isAscending;
        }

        public static IndexBuilder<TDocument> For(Expression<Func<TDocument, object>> expression,
            bool isAscending = true) => new(expression, isAscending);

        public bool IsAscending { get; set; }
        public Expression<Func<TDocument, object>> Expression { get; set; }
    }
}
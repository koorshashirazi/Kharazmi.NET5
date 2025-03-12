#region

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace Kharazmi.Extensions
{
    internal class EntityVisitor<T> : ExpressionVisitor where T : class
    {
        public T? Entity { get; private set; }

        protected override Expression VisitMember([NotNull] MemberExpression node)
        {
            var e = base.VisitMember(node);
            if (node.Expression is ConstantExpression constantExpression)
            {
                var type = constantExpression.Value?.GetType();

                var obj =
                    type?.InvokeMember(node.Member.Name, BindingFlags.GetField, null, constantExpression.Value, null);

                Entity = obj as T;
            }

            return e;
        }


        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.Right is MemberExpression rightMemberExpression) VisitMember(rightMemberExpression);

            if (node.Left is MemberExpression leftMemberExpression) VisitMember(leftMemberExpression);

            return base.VisitBinary(node);
        }
    }
}
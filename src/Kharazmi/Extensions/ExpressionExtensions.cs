#region

using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace Kharazmi.Extensions
{
    public static class ExpressionExtensions
    {
        public static T? GetObjectValue<T>(this Expression<Func<T, bool>> expr) where T : class
        {
            if (expr is null) throw new ArgumentNullException(nameof(expr));

            if (expr.Body is BinaryExpression binaryExpression)
            {
                var rightEq = (MemberExpression) binaryExpression.Right;
                var rightEqExpression = (MemberExpression) rightEq.Expression!;
                var captureConst = (ConstantExpression) rightEqExpression.Expression!;
                return ((FieldInfo) rightEqExpression.Member).GetValue(captureConst.Value) as T;
            }

            if (expr.Body is MethodCallExpression methodCallExpression)
                throw new InvalidExpressionException("Can not convert from method call");

            return default;
        }

        public static T? GetValue<T>(this Expression<Func<T, bool>> filter) where T : class
        {
            var entityVisitor = new EntityVisitor<T>();
            entityVisitor.Visit(filter);
            return entityVisitor.Entity;
        }

        public static (Expression? Left, Expression? Right) GetLeftRight<T>(this Expression<Func<T, object>> expr)
        {
            if (expr.Body is BinaryExpression be)
            {
                Expression left = be.Left;
                Expression right = be.Right;
                return (left, right);
            }

            if (expr.Body is MethodCallExpression mc && mc.Method.Name == "Equals" && mc.Arguments.Count == 1)
            {
                var left = mc.Object;
                Expression right = mc.Arguments[0];
                return (left, right);
            }

            return (default, default);
        }

        public static Expression? GetRight<T>(this Expression<Func<T, object>> expr)
        {
            if (expr.Body is BinaryExpression be)
                return be.Right;

            if (expr.Body is MemberExpression mem)
                return mem;

            if (expr.Body is MethodCallExpression mc && mc.Method.Name == "Equals" && mc.Arguments.Count == 1)
                return mc.Arguments[0];

            return default;
        }

        public static string? GetPropertyName<T>(this Expression<Func<T, object>> expr)
        {
            if (expr.Body is MemberExpression mem)
                return mem.Member.Name;

            if (expr.Body is UnaryExpression be)
            {
                if (be.Operand is MemberExpression prop)
                    return prop.Member.Name;
            }

            return default;
        }
    }
}
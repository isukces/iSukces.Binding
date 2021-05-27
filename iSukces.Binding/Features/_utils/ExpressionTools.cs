using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace iSukces.Binding
{
    internal static class ExpressionTools
    {
        // ReSharper disable once UnusedParameter.Global
        public static string GetBindingPath<T>(this T src, Expression<Func<T, object>> action)
        {
            return GetBindingPath(action);
        }

        public static string GetBindingPath<T, T2>(Expression<Func<T, T2>> action)
        {
            var parts = new List<string>();

            Expression expression = action;
            while (true)
            {
                MemberExpression memberExpression = null;
                switch (expression.NodeType)
                {
                    case ExpressionType.Lambda:
                        expression = ((LambdaExpression)expression).Body;
                        continue;
                    case ExpressionType.Convert:
                        var ue = (UnaryExpression)expression;
                        memberExpression = ue.Operand as MemberExpression;
                        break;
                    case ExpressionType.MemberAccess:
                        memberExpression = expression as MemberExpression;
                        break;
                }

                if (memberExpression == null)
                    break;
                parts.Add(memberExpression.Member.Name);
                expression = memberExpression.Expression;
            }

            parts.ReverseMe();
            return string.Join(".", parts);
        }

        public static string GetInfo<T>(Expression<Func<T, object>> action)
        {
            var parts      = new List<string>();
            var expression = GetMemberInfo(action);
            while (expression != null)
            {
                parts.Add(expression.Member.Name);
                expression = GetMemberInfo(expression.Expression);
            }

            parts.ReverseMe();
            return string.Join(".", parts);
        }

        public static string GetInfo<T, T2>(Expression<Func<T, T2>> action)
        {
            var parts      = new List<string>();
            var expression = GetMemberInfo(action);
            while (expression != null)
            {
                parts.Add(expression.Member.Name);
                expression = GetMemberInfo(expression.Expression);
            }

            parts.ReverseMe();
            return string.Join(".", parts);
        }

        [CanBeNull]
        private static MemberExpression GetMemberInfo(Expression method)
        {
            switch (method.NodeType)
            {
                case ExpressionType.Lambda:
                    return GetMemberInfo(((LambdaExpression)method).Body);
                case ExpressionType.Convert:
                    var ue = (UnaryExpression)method;
                    return ue.Operand as MemberExpression;
                case ExpressionType.MemberAccess:
                    return method as MemberExpression;
            }

            return null;
        }


        private static void ReverseMe<T>(this IList<T> x)
        {
            if (x == null || x.Count == 0) return;
            var maxIdx  = x.Count - 1;
            var maxLoop = maxIdx / 2;
            for (var index = 0; index <= maxLoop; index++)
            {
                var destIndex = maxIdx - index;
                var tmp       = x[index];
                x[index]     = x[destIndex];
                x[destIndex] = tmp;
            }
        }
    }
}

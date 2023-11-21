using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace Kirei.Repositories
{
    /// <summary>
    /// Utility service that converts an Expression of one type to an Expression of another type that
    /// is statically compatible with the properties of the type used in the original Expression.
    /// </summary>
    public class ExpressionConverter : IExpressionConverter
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="From"></typeparam>
        /// <typeparam name="To"></typeparam>
        /// <param name="from"></param>
        /// <returns></returns>
        public virtual Expression<To> Convert<From, To>(Expression<From> from)
            where From : class
            where To : class
        {
            // Work out which types are different in the function-signature
            var fromTypes = from.Type.GetGenericArguments();
            var toTypes = typeof(To).GetGenericArguments();
            if (fromTypes.Length != toTypes.Length) {
                throw new NotSupportedException("Incompatible lambda function-type signatures");
            }

            var typeMap = new Dictionary<Type, Type>();
            for (int i = 0; i < fromTypes.Length; i++) {
                if (fromTypes[i] != toTypes[i]) {
                    typeMap[fromTypes[i]] = toTypes[i];
                }
            }

            // Re-map all parameters that involve different types
            var parameterMap = new Dictionary<Expression, Expression>();
            var newParams = new ParameterExpression[from.Parameters.Count];
            for (int i = 0; i < newParams.Length; ++i) {
                Type newType;
                if (typeMap.TryGetValue(from.Parameters[i].Type, out newType)) {
                    parameterMap[from.Parameters[i]] = newParams[i] =
                        Expression.Parameter(newType, from.Parameters[i].Name);
                } else {
                    newParams[i] = from.Parameters[i];
                }
            }

            // Rebuild the lambda
            var body = new TypeConversionVisitor(parameterMap).Visit(from.Body);
            var ret = Expression.Lambda<To>(body, newParams);

            return ret;
        }

        /// <summary>
        /// ExpressionVistor that does some of the heavy lifting for us during conversion.
        /// </summary>
        private class TypeConversionVisitor : ExpressionVisitor
        {
            private readonly Dictionary<Expression, Expression> parameterMap;

            public TypeConversionVisitor(
                Dictionary<Expression, Expression> parameterMap)
            {
                this.parameterMap = parameterMap;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                // Re-map the parameter
                Expression found;
                if (!parameterMap.TryGetValue(node, out found)) {
                    found = base.VisitParameter(node);
                }
                return found;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                // Re-perform any member-binding
                var expr = Visit(node.Expression);
                if (expr != null && expr.Type != node.Type) {
                    var newMember = expr.Type.GetMember(node.Member.Name)
                                               .Single();
                    return Expression.MakeMemberAccess(expr, newMember);
                }

                return base.VisitMember(node);
            }
        }
    }
}

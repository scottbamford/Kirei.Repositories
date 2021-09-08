using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Utility methods for combining where expressions.
    /// </summary>
    public class WhereExpressionUtilities
    {
        /// <summary>
        /// Compose a combined expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="merge"></param>
        /// <returns></returns>
        public static Expression<T> Compose<T>(Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);
            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
            // apply composition of lambda expression bodies to parameters from the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        /// <summary>
        /// Compose a combined expression with And
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return Compose(first, second, Expression.And);
        }

        /// <summary>
        /// Compose a combined expression with Or
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return Compose(first, second, Expression.Or);
        }

        /// <summary>
        /// Combine multiple expressions using And.  Any null expressions are skipped.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> CombineAnd<T>(params Expression<Func<T, bool>>[] expressions)
        {
            Expression<Func<T, bool>> ret = null;
            foreach (var expression in expressions) {
                if (expression == null) {
                    continue;
                }

                if (ret == null) {
                    ret = expression;
                } else {
                    ret = WhereExpressionUtilities.And(ret, expression);
                }
            }
            return ret;
        }


        /// <summary>
        /// Combine multiple expressions using Or.  Any null expressions are skipped.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> CombineOr<T>(params Expression<Func<T, bool>>[] expressions)
        {
            Expression<Func<T, bool>> ret = null;
            foreach (var expression in expressions) {
                if (expression == null) {
                    continue;
                }

                if (ret == null) {
                    ret = expression;
                } else {
                    ret = WhereExpressionUtilities.Or(ret, expression);
                }
            }
            return ret;
        }

        /// <summary>
        /// ExpressionVisitor that will rewrite parameters as we combine them.
        /// </summary>
        public class ParameterRebinder : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

            public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }
            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            {
                return new ParameterRebinder(map).Visit(exp);
            }
            protected override Expression VisitParameter(ParameterExpression p)
            {
                if (_map.TryGetValue(p, out ParameterExpression replacement)) {
                    p = replacement;
                }
                return base.VisitParameter(p);
            }
        }
    }
}

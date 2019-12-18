using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace Kirei.Repositories
{
    /// <summary>
    /// Utility service that converts an Expression of one type to an Expression of another type that
    /// is statically compatible with the properties of the type used in the original Expression.
    /// </summary>
    public interface IExpressionConverter
    {
        /// <summary>
        /// Convert the expression <paramref name="from"/> subsituting the type <typeparamref name="From"/> with <typeparamref name="To"/>.
        /// </summary>
        /// <typeparam name="From"></typeparam>
        /// <typeparam name="To"></typeparam>
        /// <param name="from"></param>
        /// <returns></returns>
        Expression<To> Convert<From, To>(Expression<From> from)
            where From : class
            where To : class;
    }
}

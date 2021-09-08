using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Extension methods for ResolveFieldContext.
    /// </summary>
    public static class ResolveFieldContextExtensions
    {
        /// <summary>
        /// Convert a field that is a IdGraphType into one of the supported types: Guid, string, int.
        /// </summary>
        /// <typeparam name="ConvertTo"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ConvertTo ConvertIdInto<ConvertTo>(this object value)
        {
           return (ConvertTo)(object)ConversionUtilities.ConvertToType(value, typeof(ConvertTo));
        }
    }
}

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

        /// <summary>
        /// Returns the named argument as a case-insenstive dynamic dictionary that can be used as dynamic or as IDictionary&lt;string, object&gt;.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CaseInsenstiveDynamicDictionary GetArgumentAsDynamicDictionary<TSource>(this ResolveFieldContext<TSource> context, string name)
        {
            var dictionary = (IDictionary<string, object>)context.GetArgument<object>(name);
            var ret = new CaseInsenstiveDynamicDictionary(dictionary);
            return ret;
        }

        /// <summary>
        /// Returns the named argument as a case-insenstive dynamic dictionary that can be used as dynamic or as IDictionary&lt;string, object&gt;.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static dynamic GetArgumentAsDynamic<TSource>(this ResolveFieldContext<TSource> context, string name)
        {
            return GetArgumentAsDynamicDictionary<TSource>(context, name).AsDynamic();
        }
    }
}

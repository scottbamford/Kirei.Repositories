using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GraphQL;
using GraphQL.Types;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Extensions that hang off ModelRepositoryMapRequest to map repositories to models.
    /// </summary>
    public static class ModelGraphTypeMapRequestExtensions
    {
        /// <summary>
        /// Use <typeparamref name="T"/> as the GraphType for <paramref name="request"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Type Use<T>(this ModelGraphTypeMapRequest request)
        {
            return typeof(T);
        }

        /// <summary>
        /// Use <typeparamref name="Repository"/> as the repository for <paramref name="request"/>.
        /// </summary>
        /// <typeparam name="Repository"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Type Use(this ModelGraphTypeMapRequest request, Type type)
        {
            return type;
        }

        /// <summary>
        /// Use <typeparamref name="T"/> as the GraphType for <paramref name="request"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Type UseGraphTypeWithName(this ModelGraphTypeMapRequest request, string name, System.Reflection.Assembly assembly = null)
        {
            if (assembly == null) {
                assembly = request.ModelType.Assembly;
            }

            // Lookup a IGraphType with a name matching name.
            var match = assembly.GetExportedTypes()
                .Cast<Type>()
                .Where(type => type.Name == name || type.FullName == name)
                .Where(type =>
                {
                    // Must be a IGraphType to be considered.
                    var interfaces = type.GetInterfaces().Where(it => it == typeof(IGraphType));
                    return interfaces.Any();
                })
                .OrderBy(type => type.FullName == name ? 1 : 2)
                .FirstOrDefault();

            return match;
        }
    }
}

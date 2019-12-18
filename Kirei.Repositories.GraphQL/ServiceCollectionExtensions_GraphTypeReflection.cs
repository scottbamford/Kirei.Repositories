using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace GraphQL.ReflectionExtensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add GraphType registrations to <paramref name="services"/> for each IGraphType class in <paramref name="assembly"/> using <paramref name="map"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IServiceCollection AddGraphTypesForAssembly(this IServiceCollection services, Assembly assembly, string inNamespace = null)
        {
            var types = assembly.GetExportedTypes()
                .Cast<Type>()
                .Where(type =>
                {
                    // Must be a IGraphType to be considered.
                    var interfaces = type.GetInterfaces().Where(it => it == typeof(GraphQL.Types.IGraphType));
                    return interfaces.Any();
                })
                .ToList();
            foreach (var graphType in types) {
                // Make sure we are in the right namespace (if one is passed).
                if (!String.IsNullOrEmpty(inNamespace) && graphType.Namespace != inNamespace) {
                    continue;
                }

                // Don't try register anything thats not a class or is abstract.
                if (!graphType.IsClass || graphType.IsAbstract) {
                    continue;
                }

                // Make sure the type implements IGraphType.
                var interfaces = graphType.GetInterfaces().Where(it => it == typeof(GraphQL.Types.IGraphType));
                if (!interfaces.Any()) {
                    continue;
                }

                // If we get here we'll register the type.
                services.AddSingleton(graphType);
            }

            return services;
        }
    }
}

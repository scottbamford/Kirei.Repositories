using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Extensions to GraphTypeTypeRegistry that uses reflection to find and register matches for all appropriate models and graph types in an assembly.
    /// </summary>
    public static class GraphTypeTypeRegistryReflectionExtensions
    {
        
        /// <summary>
        /// Add all model types with a GraphType mapping based on <paramref name="map"/>
        /// </summary>
        /// <returns></returns>
        public static void RegisterGraphTypesForAssembly(Assembly assembly, Func<ModelGraphTypeMapRequest, Type> map)
        {
            RegisterGraphTypesForAssembly(assembly, inNamespace: null, map: map);
        }

        /// <summary>
        /// Add repositories for each model class in <paramref name="assembly"/> using <paramref name="map"/> and register any IRepositoryEvents&lt;&gt; from <paramref name="assembly"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static void RegisterGraphTypesForAssembly(Assembly assembly, string inNamespace, Func<ModelGraphTypeMapRequest, Type> map)
        {
            var types = assembly.GetExportedTypes();
            foreach (var modelType in types) {
                // Make sure we are in the right namespace (if one is passed).
                if (!String.IsNullOrEmpty(inNamespace) && modelType.Namespace != inNamespace) {
                    continue;
                }

                if (!modelType.IsClass || modelType.IsAbstract) {
                    continue;
                }

                // Make sure the type does not implements IGraphType already.
                var interfaces = modelType.GetInterfaces().Where(it => it == typeof(IGraphType));
                if (interfaces.Any()) {
                    continue;
                }

                // If we get here we're going to use map to find a match.
                // Prepare a request to pass on to the map function and let our caller map the model type to a repository as a class.
                var request = new ModelGraphTypeMapRequest
                {
                    ModelType = modelType,
                };

                // Let the caller perform the mapping.
                var graphType = map(request);

                // If we got a repository to map to, add it to the services collection.
                if (graphType != null) {
                    GraphTypeTypeRegistry.Register(modelType, graphType);
                }
            }
        }


    }
}

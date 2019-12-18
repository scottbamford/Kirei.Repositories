using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

using Kirei.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the ModelConverter and ExpressionConvert to <paramref name="services"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddModelConverter(this IServiceCollection services)
        {
            services.AddSingleton<IModelConverter, ModelConverter>();
            services.AddSingleton<IExpressionConverter, ExpressionConverter>();

            return services;
        }


        /// <summary>
        /// Add the MemoryRepositoryStore types to <paramref name="services"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMemoryRepositoryStore(this IServiceCollection services)
        {
            // Make sure the repository stores persist as singletons in memory.
            services.AddSingleton(typeof(MemoryRepositoryStore<>), typeof(MemoryRepositoryStore<>));

            return services;
        }

        /// <summary>
        /// Add <typeparamref name="Repository"/> as the repository for <typeparamref name="Model"/>.
        /// </summary>
        /// <typeparam name="Model"></typeparam>
        /// <typeparam name="Repository"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepository<Model, Repository>(this IServiceCollection services)
            where Model : class, new()
            where Repository : class, IRepository<Model>
        {
            services.AddModelConverter();
            
            services.AddTransient<IRepository<Model>, Repository>();

            return services;
        }

        /// <summary>
        /// Add <typeparamref name="Events"/> as events fore repositories for <typeparamref name="Model"/>.
        /// </summary>
        /// <typeparam name="Model"></typeparam>
        /// <typeparam name="Repository"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositoryEvents<Model, RepositoryEvents>(this IServiceCollection services)
            where Model : class, new()
            where RepositoryEvents : class, IRepositoryEvents<Model>
        {
            services.AddSingleton<IRepositoryEvents<Model>, RepositoryEvents>();
            
            return services;
        }

        /// <summary>
        /// Add a repository for <typeparamref name="Model"/> using <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="Model"></typeparam>
        /// <param name="services"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepository<Model>(this IServiceCollection services, Func<ModelRepositoryMapRequest, Type> map)
            where Model : class, new()
        {

            services.AddModelConverter();

            var request = new ModelRepositoryMapRequest
            {
                Services = services,
                ModelType = typeof(Model),
            };

            var repositoryType = map(request);
            if (repositoryType != null) {
                services.AddTransient(typeof(IRepository<Model>), repositoryType);
            }

            return services;
        }

        /// <summary>
        /// Add repositories for each model class in <paramref name="assembly"/> under <paramref name="theNamespace"/> using <paramref name="map"/> and register any IRepositoryEvents&lt;&gt; from <paramref name="assembly"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <param name="theNamespace"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositoriesForAssemblyNamespace(this IServiceCollection services, Assembly assembly, string theNamespace, Func<ModelRepositoryMapRequest, Type> map)
        {
            return AddRepositoriesForAssembly(services, assembly, (request) =>
            {
                if (request.ModelType.Namespace != theNamespace) {
                    return null;
                }

                return map(request);
            });

        }

        /// <summary>
        /// Add repositories for each model class in <paramref name="assembly"/> using <paramref name="map"/> and register any IRepositoryEvents&lt;&gt; from <paramref name="assembly"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositoriesForAssembly(this IServiceCollection services, Assembly assembly, Func<ModelRepositoryMapRequest, Type> map)
        {
            services.AddModelConverter();

            var types = assembly.GetExportedTypes();
            foreach (var modelType in types) {
                if (!modelType.IsClass) {
                    continue;
                }

                // See if the type implements IRepositoryEvent<>.
                // If it does we add it as an IRepositoryEvent<> for use by repositories of the right model type, rather than trying to register
                // a repository treating the type like a model type.
                var eventInterfaces = modelType.GetInterfaces().Where(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IRepositoryEvents<>));
                if (eventInterfaces.Any()) {
                    foreach (var eventInterface in eventInterfaces) {
                        services.AddSingleton(eventInterface, modelType);
                    }
                    continue;
                }

                // If we get here we are going to treat the type as model type, but if the type has no public propeties then ignore it
                // as it can't be a useful model.
                if (!modelType.GetProperties().Any()) {
                    continue;
                }

                // Prepare a request to pass on to the map function and let our caller map the model type to a repository as a class.
                var request = new ModelRepositoryMapRequest
                {
                    Services = services,
                    ModelType = modelType,
                };

                // Let the caller perform the mapping.
                var repositoryType = map(request);

                // If we got a repository to map to, add it to the services collection.
                if (repositoryType != null) {
                    services.AddTransient(typeof(IRepository<>).MakeGenericType(modelType), repositoryType);
                }
            }

            return services;
        }
    }
}

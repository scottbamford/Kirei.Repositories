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

            AddAllRepositoryInterfaces(services, typeof(Model), typeof(Repository));

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
            services.AddScoped<IRepositoryEvents<Model>, RepositoryEvents>();

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
                AddAllRepositoryInterfaces(services, typeof(IRepository<Model>), repositoryType);
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
        [Obsolete("This method will be removed in a future release.  Please use AddRepositoryForAssembly passing the namespace.")]
        public static IServiceCollection AddRepositoriesForAssemblyNamespace(this IServiceCollection services, Assembly assembly, string theNamespace, Func<ModelRepositoryMapRequest, Type> map)
        {
            return AddRepositoriesForAssembly(services, assembly, theNamespace, map);

        }

        /// <summary>
        /// Add repositories for each model class in <paramref name="assembly"/> under <paramref name="theNamespace"/> using <paramref name="map"/> and register any IRepositoryEvents&lt;&gt; from <paramref name="assembly"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <param name="theNamespace"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositoriesForAssembly(this IServiceCollection services, Assembly assembly, Func<ModelRepositoryMapRequest, Type> map)
        {
            return AddRepositoriesForAssembly(services, assembly, inNamespace: null, map);
        }

        /// <summary>
        /// Add repositories for each model class in <paramref name="assembly"/> using <paramref name="map"/> and register any IRepositoryEvents&lt;&gt; from <paramref name="assembly"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IServiceCollection AddRepositoriesForAssembly(this IServiceCollection services, Assembly assembly, string inNamespace, Func<ModelRepositoryMapRequest, Type> map)
        {
            services.AddModelConverter();

            var types = assembly.GetExportedTypes();
            foreach (var modelType in types) {
                if (!modelType.IsClass || modelType.IsAbstract) {
                    continue;
                }

                // Restrict by the namespace (if its been specified).
                if (!String.IsNullOrEmpty(inNamespace) && modelType.Namespace != inNamespace) {
                    continue;
                }

                // See if the type implements IRepositoryEvent<>.
                // If it does we add it as an IRepositoryEvent<> for use by repositories of the right model type, rather than trying to register
                // a repository treating the type like a model type.
                var eventInterfaces = modelType.GetInterfaces().Where(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IRepositoryEvents<>));
                if (eventInterfaces.Any()) {
                    foreach (var eventInterface in eventInterfaces) {
                        services.AddScoped(eventInterface, modelType);
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
                    AddAllRepositoryInterfaces(services, modelType, repositoryType);
                }
            }

            return services;
        }

        /// <summary>
        /// Add all repository resolutions as servies.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="modelType"></param>
        /// <param name="repositoryType"></param>
        private static void AddAllRepositoryInterfaces(IServiceCollection services, Type modelType, Type repositoryType)
        {
            // Add the normal resolution.
            services.AddScoped(typeof(IRepository<>).MakeGenericType(modelType), repositoryType);

            // If the Key property can be found on the model, also register the repository to be resolved with the PrimaryKey type specified too
            // as some code will want to be built generically around the PrimaryKey type and will want to use this version directly.
            var keyProperty = GetPrimaryKeyPropertyFromModel(modelType);
            if (keyProperty != null) {
                services.AddScoped(typeof(IRepository<,>).MakeGenericType(modelType, keyProperty.PropertyType), repositoryType);
            }
        }

        /// <summary>
        /// Returns the primary key property from <paramref name="model"/> by finding a field matching the primary key of the DbModel type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static System.Reflection.PropertyInfo GetPrimaryKeyPropertyFromModel(Type modelType)
        {
            // Lookup the key.
            var keyProperty = modelType
                .GetProperties()
                .FirstOrDefault(item => item.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), inherit: true).Any());
            if (keyProperty == null) {
                keyProperty = modelType
                    .GetProperties()
                    .FirstOrDefault(item => item.Name == "Id");
                if (keyProperty == null) {
                    keyProperty = modelType
                        .GetProperties()
                        .FirstOrDefault(item => item.Name == $"{modelType.Name}Id");

                }
            }

            return keyProperty;
        }
    }
}

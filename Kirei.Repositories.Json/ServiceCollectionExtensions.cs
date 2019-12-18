using System;
using System.Collections.Generic;
using System.Text;

using Kirei.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the JsonFileRepositoryStore to <paramref name="services"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddJsonFileRepositoriesStore(this IServiceCollection services)
        {
            return AddJsonFileRepositoriesStore(services, null);
        }

        /// <summary>
        /// Add the JsonFileRepositoryStore to <paramref name="services"/> using <paramref name="setupAction"/> to configure it.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddJsonFileRepositoriesStore(this IServiceCollection services, Action<JsonFileRepositoryStoreOptions> setupAction)
        {
            // Make sure the repository stores persist as singletons in memory.
            services.AddSingleton(typeof(JsonFileRepositoryStore<>), typeof(JsonFileRepositoryStore<>));
            
            if (setupAction != null) {
                services.Configure(setupAction);
            }

            return services;
        }
    }
}

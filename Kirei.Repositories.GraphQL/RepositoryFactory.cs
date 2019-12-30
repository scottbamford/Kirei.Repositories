using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kirei.Repositories;
using GraphQL;
using GraphQL.Types;
using GraphQL.DataLoader;
using Microsoft.Extensions.DependencyInjection;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Helper class for using repository methods to resolve perform mutations for GraphQL requests.
    /// </summary>
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public RepositoryFactory(
            IServiceProvider serviceProvider
            )
        {
            _serviceProvider = serviceProvider;
        }

        #region Async methods
        /// <summary>
        /// Create a scoped repository and perform action on it returning its result.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<Result> UseScopedRepositoryAsync<Result, Model, PrimaryKey>(Func<IRepository<Model, PrimaryKey>, Task<Result>> action)
            where Model : class
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetRequiredService<IRepository<Model, PrimaryKey>>();

                return await action(repository);
            }
        }

        /// <summary>
        /// Create a scoped repository and perform action on it returning its result.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<Result> UseScopedRepositoryAsync<Result, Model>(Func<IRepository<Model>, Task<Result>> action)
            where Model : class
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetRequiredService<IRepository<Model>>();

                return await action(repository);
            }
        }


        /// <summary>
        /// Create a scoped repository and data loader and perform action on it returning its result.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<Result> UseScopedRepositoryDataLoaderAsync<Result>(Func<IRepositoryDataLoader, Task<Result>> action)
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var dataLoader = scope.ServiceProvider.GetRequiredService<IRepositoryDataLoader>();

                return action(dataLoader);
            }
        }
        #endregion

        #region Sync methods
        /// <summary>
        /// Create a scoped repository and perform action on it returning its result.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Result UseScopedRepository<Model, PrimaryKey, Result>(Func<IRepository<Model, PrimaryKey>, Result> action)
            where Model : class
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetRequiredService<IRepository<Model, PrimaryKey>>();

                return action(repository);
            }
        }

        /// <summary>
        /// Create a scoped repository and perform action on it returning its result.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Result UseScopedRepository<Model, Result>(Func<IRepository<Model>, Result> action)
            where Model : class
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetRequiredService<IRepository<Model>>();

                return action(repository);
            }
        }
        #endregion

        /// <summary>
        /// Create a repository that is available as a Singleton.  If your repository is not available as a Singleton use UseScopedRepository() instead.
        /// </summary>
        /// <returns></returns>
        public IRepository<Model, PrimaryKey> CreateSingletonRepository<Model, PrimaryKey>()
            where Model : class
        {
            return _serviceProvider.GetRequiredService<IRepository<Model, PrimaryKey>>();
        }

        /// <summary>
        /// Create a repository that is available as a Singleton.  If your repository is not available as a Singleton use UseScopedRepository() instead.
        /// </summary>
        /// <returns></returns>
        public IRepository<Model> CreateSingletonRepository<Model>()
            where Model : class
        {
            return _serviceProvider.GetRequiredService<IRepository<Model>>();
        }


    }
}
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
    public interface IRepositoryFactory
    {
        #region Async methods
        /// <summary>
        /// Create a scoped repository and perform action on it returning its result.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<Result> UseScopedRepositoryAsync<Result, Model, PrimaryKey>(Func<IRepository<Model, PrimaryKey>, Task<Result>> action)
            where Model : class;

        /// <summary>
        /// Create a scoped repository and perform action on it returning its result.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<Result> UseScopedRepositoryAsync<Result, Model>(Func<IRepository<Model>, Task<Result>> action)
            where Model : class;

        /// <summary>
        /// Create a scoped repository and data laoader and perform action on it returning its result.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<Result> UseScopedRepositoryDataLoaderAsync<Result>(Func<IRepositoryDataLoader, Task<Result>> action);
        #endregion

        #region Sync methods
        /// <summary>
        /// Create a scoped repository and perform action on it returning its result.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Result UseScopedRepository<Model, PrimaryKey, Result>(Func<IRepository<Model, PrimaryKey>, Result> action)
            where Model : class;

        /// <summary>
        /// Create a scoped repository and perform action on it returning its result.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Result UseScopedRepository<Model, Result>(Func<IRepository<Model>, Result> action)
            where Model : class;
        #endregion

        /// <summary>
        /// Create a repository that is available as a Singleton.  If your repository is not available as a Singleton use UseScopedRepository() instead.
        /// </summary>
        /// <returns></returns>
        IRepository<Model, PrimaryKey> CreateSingletonRepository<Model, PrimaryKey>()
            where Model : class;

        /// <summary>
        /// Create a repository that is available as a Singleton.  If your repository is not available as a Singleton use UseScopedRepository() instead.
        /// </summary>
        /// <returns></returns>
        IRepository<Model> CreateSingletonRepository<Model>()
            where Model : class;


    }
}
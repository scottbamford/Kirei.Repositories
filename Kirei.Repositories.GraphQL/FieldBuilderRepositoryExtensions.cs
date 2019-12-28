using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kirei.Repositories;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.DataLoader;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Extension methods for FieldBuilder that make using the RepositoryHelper easier.
    /// </summary>
    public static class FieldBuilderRepositoryExtensions
    {
        /// <summary>
        /// Add a resolve method for finding a single item with a repository.  This is like the Repository's Find() method.
        /// </summary>
        /// <remarks>
        /// A data loader from <paramref name="repositoryDataLoader"/> is used to batch results so each repository only makes one request per batch.
        /// </remarks>
        public static FieldBuilder<TSourceType, TReturnType> ResolveWithRepositoryFindAsync<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> builder, IRepositoryDataLoader repositoryDataLoader, Func<ResolveFieldContext<TSourceType>, Expression<Func<TReturnType, bool>>> whereFromContext)
            where TReturnType : class
        {
            return ResolveWithRepositoryFindAsync<TSourceType, TReturnType, Guid>(builder, repositoryDataLoader, whereFromContext);
        }

        /// <summary>
        /// Add a resolve method for finding a single item with a repository.  This is like the Repository's Find() method.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// A data loader from <paramref name="repositoryDataLoader"/> is used to batch results so each repository only makes one request per batch.
        /// </remarks>
        public static FieldBuilder<TSourceType, TReturnType> ResolveWithRepositoryFindAsync<TSourceType, TReturnType, PrimaryKey>(this FieldBuilder<TSourceType, TReturnType> builder, IRepositoryDataLoader repositoryDataLoader, Func<ResolveFieldContext<TSourceType>, Expression<Func<TReturnType, bool>>> whereFromContext)
            where TReturnType : class
        {
            return builder
                .ResolveAsync(context =>
                    repositoryDataLoader.UseDataLoaderForFind<TReturnType, PrimaryKey>(
                        loaderKey: $"_RepositoryDataLoaderFor_${typeof(TReturnType).FullName}_Find",
                        where: whereFromContext(context)
                        )
                );
        }

        /// <summary>
        /// Add a resolve method for finding mutiple items with a repository.  This is like the Repository's FindAll() method.
        /// </summary>
        /// <remarks>
        /// A data loader from <paramref name="repositoryDataLoader"/> is used to batch results so each repository only makes one request per batch.
        /// </remarks>
        public static FieldBuilder<TSourceType, IEnumerable<TSingleReturnType>> ResolveWithRepositoryFindAllAsync<TSourceType, TSingleReturnType>(this FieldBuilder<TSourceType, IEnumerable<TSingleReturnType>> builder, IRepositoryDataLoader repositoryDataLoader, Func<ResolveFieldContext<TSourceType>, Expression<Func<TSingleReturnType, bool>>> whereFromContext = null, Expression<Func<TSingleReturnType, object>> orderBy = null, int skip = 0, int? take = null)
            where TSingleReturnType : class
        {
            return ResolveWithRepositoryFindAllAsync<TSourceType, TSingleReturnType, Guid>(builder, repositoryDataLoader, whereFromContext, orderBy, skip, take);
        }

        /// <summary>
        /// Add a resolve method for finding a multiple with a repository.  This is like the Repository's FindAll() method.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// A data loader from <paramref name="repositoryDataLoader"/> is used to batch results so each repository only makes one request per batch.
        /// </remarks>
        public static FieldBuilder<TSourceType, IEnumerable<TSingleReturnType>> ResolveWithRepositoryFindAllAsync<TSourceType, TSingleReturnType, PrimaryKey>(this FieldBuilder<TSourceType, IEnumerable<TSingleReturnType>> builder, IRepositoryDataLoader repositoryDataLoader, Func<ResolveFieldContext<TSourceType>, Expression<Func<TSingleReturnType, bool>>> whereFromContext = null, Expression<Func<TSingleReturnType, object>> orderBy = null, int skip = 0, int? take = null)
            where TSingleReturnType : class
        {
            return builder
                .ResolveAsync(context =>
                    repositoryDataLoader.UseDataLoaderForFindAll<TSingleReturnType, PrimaryKey>(
                        loaderKey: $"_RepositoryDataLoaderFor_${typeof(TSingleReturnType).FullName}_FindAll",
                        where: whereFromContext == null?
                            (item => 1 == 1 /* Default where to return everything */)
                            : whereFromContext(context),
                        orderBy: orderBy,
                        skip: skip,
                        take: take
                        )
                );
        }


        /// <summary>
        /// Perform a mutation using the repository mutator.
        /// </summary>
        /// <returns></returns>
        public static FieldBuilder<TSourceType, TReturnType> ResolveWithRepositoryAsync<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> builder, IRepositoryFactory factory, Func<ResolveFieldContext<TSourceType>, IRepository<TReturnType>, Task<TReturnType>> mutationAction)
            where TReturnType : class
        {
            return builder
                .ResolveAsync(async context => await factory.UseScopedRepositoryAsync<TReturnType, TReturnType>(repository => mutationAction(context, repository)));
        }


        /// <summary>
        /// Perform a mutation using the repository mutator.
        /// </summary>
        /// <returns></returns>
        public static FieldBuilder<TSourceType, TReturnType> ResolveWithRepository<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> builder, IRepositoryFactory factory, Func<ResolveFieldContext<TSourceType>, IRepository<TReturnType>, TReturnType> mutationAction)
            where TReturnType : class
        {
            return builder
                .Resolve(context => factory.UseScopedRepository<TReturnType, TReturnType>(repository => mutationAction(context, repository)));
        }
    }
}
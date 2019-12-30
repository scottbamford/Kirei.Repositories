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
        /// Perform a mutation or query using a repository.
        /// </summary>
        /// <returns></returns>
        public static FieldBuilder<TSourceType, TReturnType> ResolveWithRepositoryAsync<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> builder, IRepositoryFactory factory, Func<ResolveFieldContext<TSourceType>, IRepository<TReturnType>, Task<TReturnType>> resolveAction)
            where TReturnType : class
        {
            return builder
                .ResolveAsync(async context => await factory.UseScopedRepositoryAsync<TReturnType, TReturnType>(repository => resolveAction(context, repository)));
        }


        /// <summary>
        /// Perform a mutation or query using a repository.
        /// </summary>
        /// <returns></returns>
        public static FieldBuilder<TSourceType, TReturnType> ResolveWithRepository<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> builder, IRepositoryFactory factory, Func<ResolveFieldContext<TSourceType>, IRepository<TReturnType>, TReturnType> resolveAction)
            where TReturnType : class
        {
            return builder
                .Resolve(context => factory.UseScopedRepository<TReturnType, TReturnType>(repository => resolveAction(context, repository)));
        }

        /// <summary>
        /// Perform a query using a repository and a repository data laoder.
        /// </summary>
        /// <returns></returns>
        public static FieldBuilder<TSourceType, TReturnType> ResolveWithRepositoryDataLoaderAsync<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> builder, IRepositoryFactory factory, Func<ResolveFieldContext<TSourceType>, IRepositoryDataLoader, Task<TReturnType>> resolveAction)
            where TReturnType : class
        {
            return builder
                .ResolveAsync(async context => await factory.UseScopedRepositoryDataLoaderAsync<TReturnType>((dataLoader) => resolveAction(context, dataLoader)));
        }
    }
}
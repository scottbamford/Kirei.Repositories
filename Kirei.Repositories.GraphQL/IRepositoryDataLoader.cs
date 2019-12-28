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
    /// DataLoader for GraphQL resolvers that uses a repository.
    /// </summary>
    public interface IRepositoryDataLoader
    {
        /// <summary>
        /// Find one from the repository.  This is the equivalent of the Repository's Find() method.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<Model> UseDataLoaderForFind<Model, PrimaryKey>(string loaderKey, Expression<Func<Model, bool>> where)
            where Model : class;

        /// <summary>
        /// Find multiple from the repository.  This is the equivalent of the Repository's FindAll() method.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<IEnumerable<Model>> UseDataLoaderForFindAll<Model, PrimaryKey>(string loaderKey, Expression<Func<Model, bool>> where, Expression<Func<Model, object>> orderBy = null, int skip = 0, int? take = null)
            where Model : class;
    }
}
﻿using System;
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
    public class RepositoryDataLoader : IRepositoryDataLoader
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataLoaderContextAccessor _accessor;

        public RepositoryDataLoader(
            IServiceProvider serviceProvider,
            IDataLoaderContextAccessor accessor
            )
        {
            _serviceProvider = serviceProvider;
            _accessor = accessor;
        }

        /// <summary>
        /// A queued request.
        /// </summary>
        /// <typeparam name="Model"></typeparam>
        protected class DataLoaderRequest<Model, PrimaryKey>
        {
            public Expression<Func<Model, bool>> Where { get; set; }
            public Expression<Func<Model, object>> OrderBy { get; set; }
            public bool OrderByDescending { get; set; }
            public Expression<Func<Model, object>> ThenBy { get; set; }
            public bool ThenByDescending { get; set; }
            public int Skip { get; set; }
            public int? Take { get; set; }
        }

        protected string GetDefaultLoaderKey<Model>(string method)
        {
            return $"_{typeof(Model).FullName}_{method}_DataLoader";
        }


        /// <summary>
        /// Find one from the repository.  This is the equivalent of the Repository's Find() method.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IDataLoaderResult<Model> QueueFind<Model>(Expression<Func<Model, bool>> where, string loaderKey = null)
            where Model : class
        {
            return QueueFind<Model, Guid>(where, loaderKey);
        }

        /// <summary>
        /// Find one from the repository.  This is the equivalent of the Repository's Find() method.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IDataLoaderResult<Model> QueueFind<Model, PrimaryKey>(Expression<Func<Model, bool>> where, string loaderKey = null)
            where Model : class
        {
            var thisRequest = new DataLoaderRequest<Model, PrimaryKey>
            {
                Where = where,
                OrderBy = null,
                OrderByDescending = false,
                ThenBy = null,
                ThenByDescending = false,
                Skip = 0,
                Take = 1
            };

            // Get or add a batch loader with the dataLoaderName.
            // The loader will group the calls to the repository together and split the results back out again to return them.
            var loader = _accessor.Context?.GetOrAddBatchLoader<DataLoaderRequest<Model, PrimaryKey>, Model>(
                loaderKey ?? GetDefaultLoaderKey<Model>("Find"),
                async requests => {
                    var data = await LoadData(requests);
                    var ret = data.ToDictionary(
                        item => item.Key,
                        item => item.Value.FirstOrDefault()
                        );
                    return ret;
                }
                );

            // Add this request to the pending requests to fetch
            // The task will complete once the the data loader returns with the batched results
            return loader?.LoadAsync(thisRequest);
        }

        /// <summary>
        /// Find multiple from the repository.  This is the equivalent of the Repository's FindAll() method.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IDataLoaderResult<IEnumerable<Model>> QueueFindAll<Model>(Expression<Func<Model, bool>> where, Expression<Func<Model, object>> orderBy = null, bool orderByDescending = false, Expression<Func<Model, object>> thenBy = null, bool thenByDescending = false, int skip = 0, int? take = null, string loaderKey = null)
            where Model : class
        {
            return QueueFindAll<Model, Guid>(where, orderBy, orderByDescending, thenBy, thenByDescending, skip, take, loaderKey);
        }

        /// <summary>
        /// Find multiple from the repository.  This is the equivalent of the Repository's FindAll() method.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IDataLoaderResult<IEnumerable<Model>> QueueFindAll<Model, PrimaryKey>(Expression<Func<Model, bool>> where, Expression<Func<Model, object>> orderBy = null, bool orderByDescending = false, Expression<Func<Model, object>> thenBy = null, bool thenByDescending = false, int skip = 0, int? take = null, string loaderKey = null)
            where Model : class
        {
            var thisRequest = new DataLoaderRequest<Model, PrimaryKey>
            {
                Where = where,
                OrderBy = orderBy,
                OrderByDescending = orderByDescending,
                ThenBy = thenBy,
                ThenByDescending = thenByDescending,
                Skip = skip,
                Take = take
            };

            // Get or add a batch loader with the dataLoaderName.
            // The loader will group the calls to the repository together and split the results back out again to return them.
            var loader = _accessor.Context?.GetOrAddBatchLoader<DataLoaderRequest<Model, PrimaryKey>, IEnumerable<Model>>(
                loaderKey ?? GetDefaultLoaderKey<Model>("FindAll"),
                async requests =>
                {
                    var ret = await LoadData(requests);
                    return ret;
                }
                );

            // Add this request to the pending requests to fetch
            // The task will complete once the the data loader returns with the batched results
            return loader?.LoadAsync(thisRequest);
        }

        /// <summary>
        /// Function that actually executes as the data loader.
        /// </summary>
        /// <typeparam name="Model"></typeparam>
        /// <param name="requests"></param>
        /// <returns></returns>
        protected virtual async Task<Dictionary<DataLoaderRequest<Model, PrimaryKey>, IEnumerable<Model>>> LoadData<Model, PrimaryKey>(IEnumerable<DataLoaderRequest<Model, PrimaryKey>> requests)
            where Model : class
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<Model, PrimaryKey>>();

            // If we only have one request to handle, we can do everyting on the server without any fancy processing, so handle that case now.
            // (Unless we are using a feature not supported by the repository API (e.g. ThenBy), in which case we still treat it as a batch).
            if (requests.Count() == 1) {
                var request = requests.First();

                bool needCodeSideOrdering = request.ThenBy != null
                    || request.OrderByDescending;

                if (!needCodeSideOrdering) {
                    var singleResults = await repository.FindAllAsync(request.Where, request.OrderBy, request.Skip, request.Take);
                    return requests.ToDictionary(
                        item => item,
                        item => singleResults
                        );
                } else {
                    var singleResults = await repository.FindAllAsync(request.Where, request.OrderBy);

                    var batchResult = singleResults;
                    if (request.OrderBy != null) {
                        IOrderedEnumerable<Model> ordered;

                        if (request.OrderByDescending) {
                            ordered = batchResult
                                .OrderByDescending(request.OrderBy.Compile());
                        } else {
                            ordered = batchResult
                                .OrderBy(request.OrderBy.Compile());
                        }

                        if (request.ThenBy != null) {
                            if (request.ThenByDescending) {
                                ordered = ordered
                                    .ThenByDescending(request.ThenBy.Compile());
                            } else {
                                ordered = ordered
                                    .ThenBy(request.ThenBy.Compile());
                            }
                        }

                        batchResult = ordered;
                    }

                    if (request.Skip != 0) {
                        batchResult = batchResult.Skip(request.Skip);
                    }

                    if (request.Take.HasValue) {
                        batchResult = batchResult.Take(request.Take.Value);
                    }

                    return requests.ToDictionary(
                        item => item,
                        item => batchResult
                        );
                }
            }

            // Otherwise we need to read everything we require and then apply the order, skip, and take after getting the data.
            //


            // Build a combined where clause.
            Expression<Func<Model, bool>> whereAny = null;
            foreach (var request in requests) {
                if (whereAny == null) {
                    whereAny = request.Where;
                } else {
                    whereAny = WhereExpressionUtilities.Or(whereAny, request.Where);
                }
            }

            // Read all results for all batched requests.
            var results = await repository.FindAllAsync(whereAny);


            // Split the results back out by their requests, applying the order, skip, and take.
            var ret = requests.ToDictionary(
                item => item,
                item =>
                {
                    var batchResult = results.Where(item.Where.Compile());
                    if (item.OrderBy != null) {
                        IOrderedEnumerable<Model> ordered;

                        if (item.OrderByDescending) {
                            ordered = batchResult
                                .OrderByDescending(item.OrderBy.Compile());
                        } else {
                            ordered = batchResult
                                .OrderBy(item.OrderBy.Compile());
                        }

                        if (item.ThenBy != null) {
                            if (item.ThenByDescending) {
                                ordered = ordered
                                    .ThenByDescending(item.ThenBy.Compile());
                            } else {
                                ordered = ordered
                                    .ThenBy(item.ThenBy.Compile());
                            }
                        }

                        batchResult = ordered;
                    }

                    if (item.Skip != 0) {
                        batchResult = batchResult.Skip(item.Skip);
                    }

                    if (item.Take.HasValue) {
                        batchResult = batchResult.Take(item.Take.Value);
                    }

                    return batchResult;
                }
                );
            return ret;
        }
    }
}

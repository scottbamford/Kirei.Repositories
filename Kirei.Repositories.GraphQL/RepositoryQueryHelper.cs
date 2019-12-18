using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kirei.Repositories;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Helper class for Query methods that internally use a IRepository to perform the actions.
    /// </summary>
    /// <remarks>
    /// This API has been designed to let you use it always by adding the helper methods as a resolve action for a Field().
    /// 
    /// The API can also serve as a fallback for when data has not otherwise been loaded by alternative methods such as GraphQL.EntityFramework.
    /// 
    /// To use with GraphQL.EntityFramework's navigation properties and only fall-back on the repository if the value isn't loaded pre-loaded by EntityFramework you can do something along the lines of:
    /// <code>
    /// // For lists:
    ///  AddNavigationListField("subCategories", resolve: context => context.Source.SubCategories ?? subCategoryQueryHelper.FindAll(item => item.CategoryId == context.Source.Id), includeNames: new[] { "SubCategories" });
    ///  AddNavigationConnectionField("subCategoriesConnection", resolve: context => context.Source.ySubCategories ?? sCategoryQueryHelper.FindAll(item => item.CategoryId == context.Source.Id), includeNames: new[] { "SubCategories" });
    ///  
    /// // For single navigation:
    /// AddNavigationField(name: "category", resolve: context => context.Source.Category ?? propertyQueryHelper.Find(context.Source.CategoryId),  includeNames: new[] { "Category" });
    /// </code>
    /// </remarks>
    public class RepositoryQueryHelper<Model> : RepositoryQueryHelper<Model, Guid>
        where Model : class
    {
        public RepositoryQueryHelper(
            IServiceProvider serviceProvider
            )
            : base(serviceProvider)
        {
        }
    }

    /// <summary>
    /// Query methods that internally use a IRepository to perform the actions.
    /// </summary>
    public class RepositoryQueryHelper<Model, PrimaryKey>
        where Model: class
    {
        private readonly IServiceProvider _serviceProvider;

        public RepositoryQueryHelper(
            IServiceProvider serviceProvider
            )
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Find an item from the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Model Find(PrimaryKey key)
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetService<IRepository<Model, PrimaryKey>>();

                // Find the model.
                var model = repository.Find(key);
                return model;
            }
        }

        /// <summary>
        /// Find an item from the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Model Find(Expression<Func<Model, bool>> where = null)
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetService<IRepository<Model, PrimaryKey>>();

                // Find the model.
                var model = repository.Find(where);
                return model;
            }
        }

        /// <summary>
        /// Find items from the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<Model> FindAll(Expression<Func<Model, bool>> where = null, int skip = 0, int? take = null)
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetService<IRepository<Model, PrimaryKey>>();

                // Find the models
                var models = repository.FindAll(where, skip, take);
                return models;
            }
        }

        /// <summary>
        /// Find items from the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<Model> FindAll<TKey>(Expression<Func<Model, bool>> where = null, Expression<Func<Model, TKey>> orderBy = null, int skip = 0, int? take = null)
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetService<IRepository<Model, PrimaryKey>>();

                // Find the models
                var models = repository.FindAll(where, orderBy, skip, take);
                return models;
            }
        }
    }
}

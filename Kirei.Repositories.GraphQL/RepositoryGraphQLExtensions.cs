using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Extension methods for working with repositories more easily with GraphQL dotnet.
    /// </summary>
    public static class RepositoryGraphQLExtensions
    {
        /// <summary>
        /// Mutation to create a model in the repository and save it after applying changes.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<Model> CreateMutationAsync<Model, PrimaryKey>(this IRepository<Model, PrimaryKey> repository, object changes)
            where Model: class
        {
            // Create the model.
            var model = await repository.CreateAsync();

            // Copy across all changed fields.
            ConversionUtilities.ApplyChanges(model, changes);

            // Save the changes.
            await repository.SaveAsync(model);

            return model;
        }

        /// <summary>
        /// Mutation to update a model in the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static async Task<Model> UpdateMutationAsync<Model, PrimaryKey>(this IRepository<Model, PrimaryKey> repository, PrimaryKey id, object changes)
            where Model : class
        {
            return await SaveChangesMutationAsync(repository, id, changes);
        }

        /// <summary>
        /// Mutation to save a set of changes against a model in the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static async Task<Model> SaveChangesMutationAsync<Model, PrimaryKey>(this IRepository<Model, PrimaryKey> repository, PrimaryKey id, object changes)
            where Model : class
        {
            // Find the model.
            var model = await repository.FindAsync(id);

            // Copy across all changed fields.
            ConversionUtilities.ApplyChanges(model, changes);

            // Save the changes.
            await repository.SaveAsync(model);

            return model;
        }

        /// <summary>
        /// Mutation to delete a model in the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static async Task<Model> RemoveAsync<Model, PrimaryKey>(this IRepository<Model, PrimaryKey> repository, PrimaryKey id)
            where Model : class
        {
            // Remove the item.
            await repository.RemoveAsync(id);

            // Return null to remove the item from any caches.
            return default;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Extension methods for working with repositories more easily with GraphQL dotnet.
    /// </summary>
    public static class RepositoryGraphQLMutationExtensions
    {
        /// <summary>
        /// Mutation to create a model in the repository and save it after applying changes.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<Model> CreateMutationAsync<Model, PrimaryKey>(this IRepository<Model, PrimaryKey> repository, object changes, Func<Model, bool> validate = null)
            where Model: class
        {
            // Create the model.
            var model = await repository.CreateAsync();

            // Copy across all changed fields.
            ConversionUtilities.ApplyChanges(model, changes);

            // Validate the model before saving to allow any business rules to be applied.
            if (validate != null) {
                if (!validate(model)) {
                    return default;
                }
            }

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
        public static async Task<Model> UpdateMutationAsync<Model, PrimaryKey>(this IRepository<Model, PrimaryKey> repository, PrimaryKey id, object changes, Func<Model, bool> validate = null)
            where Model : class
        {
            return await SaveChangesMutationAsync(repository, id, changes, validate);
        }

        /// <summary>
        /// Mutation to save a set of changes against a model in the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public static async Task<Model> SaveChangesMutationAsync<Model, PrimaryKey>(this IRepository<Model, PrimaryKey> repository, PrimaryKey id, object changes, Func<Model, bool> validate = null)
            where Model : class
        {
            // Find the model.
            var model = await repository.FindAsync(id);

            // Copy across all changed fields.
            ConversionUtilities.ApplyChanges(model, changes);

            // Validate the model before saving to allow any business rules to be applied.
            if (validate != null) {
                if (!validate(model)) {
                    return default;
                }
            }

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
        public static async Task<Model> RemoveMutationAsync<Model, PrimaryKey>(this IRepository<Model, PrimaryKey> repository, PrimaryKey id, Func<Model, bool> validate = null)
            where Model : class
        {
            // Validate the model before saving to allow any business rules to be applied.
            if (validate != null) {
                var model = await repository.FindAsync(id);

                if (!validate(model)) {
                    return default;
                }
            }

            // Remove the item.
            await repository.RemoveAsync(id);

            // Return null to remove the item from any caches.
            return default;
        }
    }
}

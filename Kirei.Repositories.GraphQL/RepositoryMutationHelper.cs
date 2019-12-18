using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kirei.Repositories;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Helper class for Mutations methods that internally use a IRepository to perform the actions.
    /// </summary>
    public class RepositoryMutationHelper<Model>
        where Model: class
    {
        private readonly IServiceProvider _serviceProvider;

        public RepositoryMutationHelper(
            IServiceProvider serviceProvider
            )
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Apply <paramref name="changes"/> to <paramref name="model"/>.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="changes"></param>
        private static void ApplyChanges(Model model, IDictionary<string, object> changes)
        {
            foreach (var change in changes) {
                var property = FindProperty(model, change.Key);
                property.SetValue(model, change.Value);
            }
        }

        /// <summary>
        /// Returns a property for <paramref name="name"/> on <paramref name="obj"/> or null if no property could be found.
        /// </summary>
        /// <remarks>
        /// When matching a property <paramref name="name"/> is treated case insensitive, however if there is more than once match, a case sensitive match is always preferred.
        /// </remarks>
        /// <returns></returns>
        private static System.Reflection.PropertyInfo FindProperty(object obj, string name)
        {
            var type = obj.GetType();
            var property = type.GetProperty(name);
            if (property == null) {
                property = type.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            }

            return property;
        }

        /// <summary>
        /// Mutation to create a model in the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Model Create(ResolveFieldContext<object> context)
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetService<IRepository<Model>>();

                var input = (IDictionary<string, object>)context.GetArgument<object>("model");
                var id = input.ContainsKey("id") ? (Guid?)input["id"] : (Guid?)null;

                // Create the model.
                var model = repository.Create();

                // Create or assign a unique Id.
                var idProperty = FindProperty(model, "id");
                idProperty.SetValue(model, id.HasValue ? id.Value : Guid.NewGuid());

                // Copy across all changed fields.
                ApplyChanges(model, input);

                // Save the changes.
                repository.Save(model);

                return model;
            }
        }

        /// <summary>
        /// Mutation to update a model in the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public Model Update(ResolveFieldContext<object> context)
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetService<IRepository<Model>>();

                var input = (IDictionary<string, object>)context.GetArgument<object>("model");
                var id = (Guid)input["id"];

                // Find the model.
                var model = repository.Find(id);

                // Copy across all changed fields.
                ApplyChanges(model, input);

                // Save the changes.
                repository.Save(model);
                
                return model;
            }
        }

        /// <summary>
        /// Mutation to delete a model in the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public Model Delete(ResolveFieldContext<object> context)
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetService<IRepository<Model>>();

                var id = context.GetArgument<Guid>("id");

                // Lookup the model (we'll return it after deletion as the result of the mutation).
                var model = repository.Find(id);

                // Remove the item.
                repository.Remove(id);

                return model;
            }
        }

        /// <summary>
        /// Mutation to archive a model in the repository.  Convinient drop-in altnerative to Delete when data should be archived by editing the model rather than removing it from the repository.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public Model Archive(ResolveFieldContext<object> context, Action<Model> archiveAction)
        {
            using (var scope = _serviceProvider.CreateScope()) {
                var repository = scope.ServiceProvider.GetService<IRepository<Model>>();

                var id = context.GetArgument<Guid>("id");

                // Lookup the model (we'll return it after deletion as the result of the mutation).
                var model = repository.Find(id);

                // Update into the archived state using the passed in delegate.
                archiveAction(model);

                // Save the change.
                repository.Save(model);

                return model;
            }
        }
    }
}

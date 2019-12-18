using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Kirei.Repositories
{
    /// <summary>
    /// Extensions that hang off ModelRepositoryMapRequest to map repositories to models.
    /// </summary>
    public static class ModelRepositoryMapRequestExtensions
    {
        /// <summary>
        /// Use <typeparamref name="Repository"/> as the repository for <paramref name="request"/>.
        /// </summary>
        /// <typeparam name="Repository"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Type Use<Repository>(this ModelRepositoryMapRequest request)
        {
            return typeof(Repository);
        }

        /// <summary>
        /// Use MemoryRepository <typeparamref name="Repository"/> as the repository for <paramref name="request"/>.
        /// </summary>
        /// <typeparam name="Repository"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Type UseMemoryStore(this ModelRepositoryMapRequest request)
        {
            request.Services.AddMemoryRepositoryStore();

            return typeof(MemoryRepository<>).MakeGenericType(request.ModelType); // TODO cope with primary key types other than Guid.
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Kirei.Repositories
{
    /// <summary>
    /// Extensions that hang off ModelRepositoryMapRequest to map JsonFileRepositoryStore repositories to models.
    /// </summary>
    public static class JsonFileRepositoryMapRequestExtensions
    {
        /// <summary>
        /// Use JsonFileRepository <typeparamref name="Repository"/> as the repository for <paramref name="request"/>.
        /// </summary>
        /// <typeparam name="Repository"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Type UseJsonFileStore(this ModelRepositoryMapRequest request)
        {
            return UseJsonFileStore(request, setupAction: null);
        }

        /// <summary>
        /// Use JsonFileRepository <typeparamref name="Repository"/> as the repository for <paramref name="request"/>.
        /// </summary>
        /// <typeparam name="Repository"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Type UseJsonFileStore(this ModelRepositoryMapRequest request, Action<JsonFileRepositoryStoreOptions> setupAction)
        {
            request.Services.AddJsonFileRepositoriesStore();

            var ret = typeof(JsonFileRepository<>).MakeGenericType(request.ModelType); // TODO cope with primary key types other than Guid.
            
            if (setupAction != null) {
                request.Services.Configure(setupAction);
            }

            return ret;
        }
    }
}

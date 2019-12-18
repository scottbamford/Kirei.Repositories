using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Kirei.Repositories
{
    /// <summary>
    /// Request to map the ModelType to 
    /// </summary>
    /// <remarks>
    /// The primary purpose of this class rather than just using Type in the API is so we can hang extension methods off it in the
    /// style .UseXXX() e.g. UseMemoryStore().
    /// </remarks>
    public class ModelRepositoryMapRequest
    {
        /// <summary>
        /// Type of the Model we want to map a repository for.
        /// </summary>
        public Type ModelType { get; set; }

        /// <summary>
        /// IServiceCollection that can be used to register any supporting types needed by the UseXXX() extensions repository type.
        /// </summary>
        public IServiceCollection Services { get; set; }
    }
}

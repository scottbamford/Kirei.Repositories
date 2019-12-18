using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Request to map the ModelType to 
    /// </summary>
    /// <remarks>
    /// The primary purpose of this class rather than just using Type in the API is so we can hang extension methods off it in the
    /// style .UseXXX().
    /// </remarks>
    public class ModelGraphTypeMapRequest
    {
        /// <summary>
        /// Type of the Model we want to map a GraphType for.
        /// </summary>
        public Type ModelType { get; set; }
    }
}

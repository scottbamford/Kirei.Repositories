using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kirei.Repositories
{
    /// <summary>
    /// Store for persistace of data by MemoryStoreRepository that can be used to store the data between sessions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    public interface IRepositoryStore<T>
    {
        /// <summary>
        /// <summary>
        /// Save data to the data store.
        /// </summary>
        Task<bool> SaveAsync();

        /// <summary>
        /// Gets the in memory version of the data in the store.
        /// </summary>
        /// <remarks>
        /// Does not automatically call LoadAsync() or SaveAsync() you have to call these when you need them.
        /// </remarks>
        IList<T> Data { get; }
    }
}

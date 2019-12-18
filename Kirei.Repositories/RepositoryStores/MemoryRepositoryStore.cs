using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kirei.Repositories
{
    /// <summary>
    /// IMemoryRepositoryStore that doesn't do any persistance between sessions and always returns no data on load and does nothing on save.
    /// </summary>
    public class MemoryRepositoryStore<T> : IRepositoryStore<T>
    {
        private IList<T> _data;

        public MemoryRepositoryStore()
        {
            _data = new List<T>();
        }

        public Task<bool> SaveAsync()
        {
            return Task.FromResult(true);
        }

        public IList<T> Data
        {
            get
            {
                return _data;
            }
        }
    }
}

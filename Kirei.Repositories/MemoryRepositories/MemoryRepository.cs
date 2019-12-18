using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kirei.Repositories
{
    /// <summary>
    /// In memory impelemation of IRepository that does not store its items in memory and can persist it between sessions.
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    public class MemoryRepository<Model> : MemoryRepository<Model, Guid>, IRepository<Model>
        where Model : class, new()
    {
        public MemoryRepository(
            MemoryRepositoryStore<Model> store,
            IEnumerable<IRepositoryEvents<Model>> events,
            IModelConverter modelConverter
            )
            : base(store, events, modelConverter)
        {
        }
    }

    /// <summary>
    /// In memory impelemation of IRepository that does not store its items in memory and can persist it between sessions.
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <typeparam name="PrimaryKey"></typeparam>
    public class MemoryRepository<Model, PrimaryKey> : MemoryStoreRepository<Model, PrimaryKey, MemoryRepositoryStore<Model>>
        where Model : class, new()
    {
        public MemoryRepository(
            MemoryRepositoryStore<Model> store,
            IEnumerable<IRepositoryEvents<Model>> events,
            IModelConverter modelConverter
            )
            : base(store, events, modelConverter)
        {
        }
    }
}

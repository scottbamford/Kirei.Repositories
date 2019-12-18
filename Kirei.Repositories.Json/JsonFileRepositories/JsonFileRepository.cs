using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kirei.Repositories
{
    /// <summary>
    /// Json impelemation of IRepository that store its items in memory and can persist in json files between sessions.
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <remarks>
    /// Use IOptions&lgt;JsonFileRepositoryStoreOptions&gt; to configure where the Json files are stored (default is Resources).
    /// </remarks>
    public class JsonFileRepository<Model> : JsonFileRepository<Model, Guid>, IRepository<Model>
        where Model : class, new()
    {
        public JsonFileRepository(
            JsonFileRepositoryStore<Model> store,
            IEnumerable<IRepositoryEvents<Model>> events,
            IModelConverter modelConverter
            )
            : base(store, events, modelConverter)
        {
        }
    }

    /// <summary>
    /// Json impelemation of IRepository that store its items in memory and can persist in json files between sessions.
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <typeparam name="PrimaryKey"></typeparam>
    /// <remarks>
    /// Use IOptions&lgt;JsonFileRepositoryStoreOptions&gt; to configure where the Json files are stored (default is Resources).
    /// </remarks>
    public class JsonFileRepository<Model, PrimaryKey> : MemoryStoreRepository<Model, PrimaryKey, JsonFileRepositoryStore<Model>>
        where Model : class, new()
    {
        public JsonFileRepository(
            JsonFileRepositoryStore<Model> store,
            IEnumerable<IRepositoryEvents<Model>> events,
            IModelConverter modelConverter
            )
            : base(store, events, modelConverter)
        {
        }
    }
}

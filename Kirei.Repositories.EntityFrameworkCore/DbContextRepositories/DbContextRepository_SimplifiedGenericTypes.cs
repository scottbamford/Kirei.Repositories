using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Kirei.Repositories;

namespace Kirei.Repositories
{
    /// <summary>
    /// Implementation of IRepository that uses EntityFraeworkCore via DbContext for all data operations.
    /// </summary>
    /// <typeparam name="DbContext"></typeparam>
    /// <typeparam name="DbModel"></typeparam>
    /// <typeparam name="Model"></typeparam>
    public class DbContextRepository<Model, DbContext, DbModel> : DbContextRepository<Model, Guid, DbContext, DbModel>, IRepository<Model>
        where DbContext : Microsoft.EntityFrameworkCore.DbContext
        where DbModel : class, new()
        where Model : class, new()
    {
        public DbContextRepository(
            DbContext context,
            IEnumerable<IRepositoryEvents<Model>> events,
            IModelConverter modelConverter,
            IExpressionConverter expressionConverter
            )
            : base(context, events, modelConverter, expressionConverter)
        {
        }
    }

    /// <summary>
    /// Implementation of IRepository that uses EntityFraeworkCore via DbContext for all data operations.
    /// </summary>
    /// <typeparam name="DbContext"></typeparam>
    /// <typeparam name="DbModel"></typeparam>
    /// <typeparam name="Model"></typeparam>
    public class DbContextRepository<Model, DbContext> : DbContextRepository<Model, Guid, DbContext, Model>, IRepository<Model>
        where DbContext : Microsoft.EntityFrameworkCore.DbContext
        where Model : class, new()
    {
        public DbContextRepository(
            DbContext context,
            IEnumerable<IRepositoryEvents<Model>> events,
            IModelConverter modelConverter,
            IExpressionConverter expressionConverter
            )
            : base(context, events, modelConverter, expressionConverter)
        {
        }
    }
}

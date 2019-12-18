using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Kirei.Repositories
{
    /// <summary>
    /// Repository that abstracts CRUD operations for models.
    /// </summary>
    public interface IRepository<T> : IRepository<T, Guid>
        where T : class
    {
    }

    /// <summary>
    /// Repository that abstracts CRUD operations for models.
    /// </summary>
    public interface IRepository<T, PrimaryKey> where T : class
    {
        T Create(PrimaryKey id = default(PrimaryKey));

        Task<T> CreateAsync(PrimaryKey id = default(PrimaryKey));

        T Find(PrimaryKey id);

        Task<T> FindAsync(PrimaryKey id);

        bool Save(T model);

        Task<bool> SaveAsync(T model);

        bool Remove(PrimaryKey id);

        Task<bool> RemoveAsync(PrimaryKey id);

        T Find(Expression<Func<T, bool>> where = null);
        T Find<TKey>(Expression<Func<T, bool>> where = null, Expression<Func<T, TKey>> orderBy = null);
        IEnumerable<T> FindAll(Expression<Func<T, bool>> where = null, int skip = 0, int? take = null);
        IEnumerable<T> FindAll<TKey>(Expression<Func<T, bool>> where = null, Expression<Func<T, TKey>> orderBy = null, int skip = 0, int? take = null);

        Task<T> FindAsync(Expression<Func<T, bool>> where = null);
        Task<T> FindAsync<TKey>(Expression<Func<T, bool>> where = null, Expression<Func<T, TKey>> orderBy = null);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> where = null, int skip = 0, int? take = null);
        Task<IEnumerable<T>> FindAllAsync<TKey>(Expression<Func<T, bool>> where = null, Expression<Func<T, TKey>> orderBy = null, int skip = 0, int? take = null);

        int Count(Expression<Func<T, bool>> where = null, int skip = 0, int? take = null);
        Task<int> CountAsync(Expression<Func<T, bool>> where = null, int skip = 0, int? take = null);
    }
}

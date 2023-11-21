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
    /// <remarks>
    /// If <typeparamref name="Model"/> and <typeparamref name="DbModel"/> are differnt types, the repository will take care of converting the types
    /// for you based on static property mappings using IModelConveter.  If <typeparamref name="Model"/> and <typeparamref name="DbModel"/> are the same
    /// type then the Repository will "pass through" the results without attempting any conversion.
    /// </remarks>
    /// <typeparam name="DbContext"></typeparam>
    /// <typeparam name="DbModel"></typeparam>
    /// <typeparam name="Model"></typeparam>
    /// <typeparam name="PrimaryKey"></typeparam>
    public class DbContextRepository<Model, PrimaryKey, DbContext, DbModel> : IRepository<Model, PrimaryKey>
        where DbContext : Microsoft.EntityFrameworkCore.DbContext
        where DbModel : class, new()
        where Model : class, new()
    {
        private readonly DbContext _context;
        private readonly IEnumerable<IRepositoryEvents<Model>> _events;
        private readonly IModelConverter _modelConverter;
        private readonly IExpressionConverter _expressionConverter;

        public DbContextRepository(
            DbContext context,
            IEnumerable<IRepositoryEvents<Model>> events,
            IModelConverter modelConverter,
            IExpressionConverter expressionConverter
            )
        {
            _context = context;
            _events = events;
            _modelConverter = modelConverter;
            _expressionConverter = expressionConverter;
        }

        public virtual Task<Model> CreateAsync(PrimaryKey id = default(PrimaryKey))
        {
            var model = new Model();
            SetPrimaryKey(model, id);
            SetNullStringsToEmpty(model);

            foreach (var eventx in _events) {
                eventx.Created(model);
            }

            return Task.FromResult(model);
        }

        public virtual async Task<Model> FindAsync(PrimaryKey id)
        {
            var dbModel = await _context.FindAsync<DbModel>(id);
            Model model;
            if (typeof(Model) == typeof(DbModel)) {
                model = (Model)(object)dbModel;
            } else {
                model = _modelConverter.CopyProperties(dbModel, new Model());
            }

            foreach (var eventx in _events) {
                eventx.Found(model);
            }

            return model;
        }

        /// <summary>
        /// Returns the primary key from <paramref name="model"/> by finding a field matching the primary key of the DbModel type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected PrimaryKey GetPrimaryKey(Model model)
        {
            var ret = (PrimaryKey)GetPrimaryKeyPropertyFromModel(model).GetValue(model);
            return ret;
        }

        /// <summary>
        /// Returns the primary key from <paramref name="model"/> by finding a field matching the primary key of the DbModel type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected void SetPrimaryKey(Model model, PrimaryKey id)
        {
            GetPrimaryKeyPropertyFromModel(model).SetValue(model, id);
        }

        /// <summary>
        /// Returns the primary key property from <paramref name="model"/> by finding a field matching the primary key of the DbModel type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected System.Reflection.PropertyInfo GetPrimaryKeyPropertyFromModel(Model model)
        {
            // Lookup the key.
            var keyProperty = typeof(DbModel)
                .GetProperties()
                .FirstOrDefault(item => item.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), inherit: true).Any());
            if (keyProperty == null) {
                keyProperty = typeof(DbModel)
                    .GetProperties()
                    .FirstOrDefault(item => item.Name == "Id");
                if (keyProperty == null) {
                    keyProperty = typeof(DbModel)
                        .GetProperties()
                        .FirstOrDefault(item => item.Name == $"{typeof(DbModel).Name}Id");

                }
            }

            if (keyProperty == null) {
                throw new InvalidOperationException($"{typeof(DbModel).FullName} has no Key property defined.");
            }

            var ret = typeof(Model).GetProperty(keyProperty.Name);
            return ret;
        }

        /// <summary>
        /// Set all null strings in the model to be String.Empty.
        /// </summary>
        /// <param name="model"></param>
        protected void SetNullStringsToEmpty(Model model)
        {
            // The ordinary defaults from the constructor/framwork will be fine, with the exception that we set all strings to Empty instead of null as most client code and database
            // code want this in normal database objects.
            var properties = typeof(Model).GetProperties();
            foreach (var property in properties) {
                if (property.PropertyType == typeof(string)) {
                    var val = property.GetValue(model);
                    if (val == null) {
                        property.SetValue(model, String.Empty);
                    }
                }
            }
        }

        public async Task<bool> SaveAsync(Model model)
        {
            foreach (var eventx in _events) {
                eventx.Saving(model);
            }

            // Find the item in the database.
            var id = GetPrimaryKey(model);
            bool isCreate = false;
            var dbModel = _context.Find<DbModel>(id);
            if (dbModel == null) {
                isCreate = true;
                dbModel = new DbModel();
            }

            _modelConverter.CopyProperties(model, dbModel);

            if (isCreate) {
                await _context.AddAsync(dbModel);
            } else {
                _context.Update(dbModel);
            }

            await _context.SaveChangesAsync();

            foreach (var eventx in _events) {
                eventx.Saved(model);
            }

            // Do nothing.
            return true;
        }

        public virtual async Task<bool> RemoveAsync(PrimaryKey id)
        {
            var dbModel = await _context.FindAsync<DbModel>(id);
            if (dbModel == null) {
                return false;
            }

            _context.Remove(dbModel);
            await _context.SaveChangesAsync();

            if (_events.Any()) {
                Model model;
                if (typeof(Model) == typeof(DbModel)) {
                    model = (Model)(object)dbModel;
                } else {
                    model = _modelConverter.CopyProperties(dbModel, new Model());
                }

                foreach (var eventx in _events) {
                    eventx.Removed(model);
                }
            }

            return true;
        }

        public async Task<Model> FindAsync(Expression<Func<Model, bool>> where = null)
        {
            var ret = await FindAllAsync(where, take: 1);
            return ret.FirstOrDefault();
        }
        public async Task<Model> FindAsync<TKey>(Expression<Func<Model, bool>> where = null, Expression<Func<Model, TKey>> orderBy = null)
        {
            var ret = await FindAllAsync(where, orderBy, take: 1);
            return ret.FirstOrDefault();
        }

        public async Task<IEnumerable<Model>> FindAllAsync(Expression<Func<Model, bool>> where = null, int skip = 0, int? take = null)
        {
            return await FindAllAsync<object>(where: where, orderBy: null, skip: skip, take: take);
        }

        public async virtual Task<IEnumerable<Model>> FindAllAsync<TKey>(Expression<Func<Model, bool>> where = null, Expression<Func<Model, TKey>> orderBy = null, int skip = 0, int? take = null)
        {
            // Convert the expressions so we can use them with the database.
            Expression<Func<DbModel, bool>> dbWhere = null;
            if (where != null) {
                dbWhere = _expressionConverter.Convert<Func<Model, bool>, Func<DbModel, bool>>(where);
            }

            Expression<Func<DbModel, TKey>> dbOrderBy = null;
            if (orderBy != null) {
                dbOrderBy = _expressionConverter.Convert<Func<Model, TKey>, Func<DbModel, TKey>>(orderBy);
            }

            // Find the data set to work with.
            IQueryable<DbModel> dbSet = _context.Set<DbModel>();

            // Apply the where clause.
            if (dbWhere != null) {
                dbSet = dbSet.Where(dbWhere);
            }

            // Apply order by clause.
            if (dbOrderBy != null) {
                dbSet = dbSet.OrderBy(dbOrderBy);
            }

            // Apply skip
            if (skip > 0) {
                dbSet = dbSet.Skip(skip);
            }

            // Take only the number of results requested.
            if (take.HasValue) {
                dbSet = dbSet.Take(take.Value);
            }

            // Read the data.
            //var dbResults = dbSet.ToAsyncEnumerable();
            var dbResults = dbSet.ToList();

            // Convert back to the model format.
            var ret = /*await*/ dbResults.Select(dbModel =>
            {
                Model model;
                if (typeof(Model) == typeof(DbModel)) {
                    model = (Model)(object)dbModel;
                } else {
                    model = _modelConverter.CopyProperties(dbModel, new Model());
                }

                foreach (var eventx in _events) {
                    eventx.Found(model);
                }

                return model;
            }).ToList();

            return await Task.FromResult(ret);
        }

        public virtual Task<int> CountAsync(Expression<Func<Model, bool>> where = null, int skip = 0, int? take = null)
        {
            // Convert the expressions so we can use them with the database.
            Expression<Func<DbModel, bool>> dbWhere = null;
            if (where != null) {
                dbWhere = _expressionConverter.Convert<Func<Model, bool>, Func<DbModel, bool>>(where);
            }

            // Find the data set to work with.
            IQueryable<DbModel> dbSet = _context.Set<DbModel>();

            // Apply the where clause.
            if (dbWhere != null) {
                dbSet = dbSet.Where(dbWhere);
            }

            // Apply skip
            if (skip > 0) {
                dbSet = dbSet.Skip(skip);
            }

            // Take only the number of results requested.
            if (take.HasValue) {
                dbSet = dbSet.Take(take.Value);
            }

            // Count the records.
            var ret = dbSet.Count();

            return Task.FromResult(ret);
        }
    }
}

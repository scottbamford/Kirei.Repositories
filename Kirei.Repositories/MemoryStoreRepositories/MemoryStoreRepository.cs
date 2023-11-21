using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kirei.Repositories
{
    /// <summary>
    /// In memory impelemation of IRepository that stores its items in memory and can persist it between sessions.
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    public class MemoryStoreRepository<Model, Store> : MemoryStoreRepository<Model, Guid, Store>, IRepository<Model>
        where Store : IRepositoryStore<Model>
        where Model : class, new()
    {
        public MemoryStoreRepository(
            Store store,
            IEnumerable<IRepositoryEvents<Model>> events,
            IModelConverter modelConverter
            )
            : base(store, events, modelConverter)
        {
        }
    }

    /// <summary>
    /// In memory impelemation of IRepository that stores its items in memory and can persist it between sessions.
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    /// <typeparam name="PrimaryKey"></typeparam>
    public class MemoryStoreRepository<Model, PrimaryKey, Store> : IRepository<Model, PrimaryKey>
        where Store : IRepositoryStore<Model>
        where Model : class, new()
    {
        private readonly Store _store;
        private readonly IEnumerable<IRepositoryEvents<Model>> _events;
        private readonly IModelConverter _modelConverter;

        public MemoryStoreRepository(
            Store store,
            IEnumerable<IRepositoryEvents<Model>> events,
            IModelConverter modelConverter
            )
        {
            _store = store;
            _events = events;
            _modelConverter = modelConverter;
        }

        /// <summary>
        /// Finds and returns the model from _data by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual Model FindDbModelInDataById(PrimaryKey id)
        {
            var primaryKeyProperty = GetPrimaryKeyPropertyFromModel();
            var dbModel = _store.Data.FirstOrDefault(item => primaryKeyProperty?.GetValue(item)?.Equals(id) ?? false);
            return dbModel;
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

        public virtual Task<Model> FindAsync(PrimaryKey id)
        {
            var dbModel = FindDbModelInDataById(id);

            // Duplicate into a seperate object so change made are not reflected back into _data without calling Save().
            var model = _modelConverter.CopyProperties(dbModel, new Model());

            foreach (var eventx in _events) {
                eventx.Found(model);
            }

            return Task.FromResult(model);
        }

        /// <summary>
        /// Returns the primary key from <paramref name="model"/> by finding a field matching the primary key of the DbModel type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected PrimaryKey GetPrimaryKey(Model model)
        {
            var ret = (PrimaryKey)GetPrimaryKeyPropertyFromModel().GetValue(model);
            return ret;
        }

        /// <summary>
        /// Sets the primary key from <paramref name="model"/> by finding a field matching the primary key of the DbModel type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected void SetPrimaryKey(Model model, PrimaryKey id)
        {
            GetPrimaryKeyPropertyFromModel().SetValue(model, id);
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

        /// <summary>
        /// Returns the primary key property from <paramref name="model"/> by finding a field matching the primary key of the DbModel type.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected System.Reflection.PropertyInfo GetPrimaryKeyPropertyFromModel()
        {
            var type = typeof(Model);

            // Lookup the key.
            var keyProperty = type
                .GetProperties()
                .FirstOrDefault(item => item.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), inherit: true).Any());
            if (keyProperty == null) {
                keyProperty = type
                    .GetProperties()
                    .FirstOrDefault(item => item.Name == "Id");
                if (keyProperty == null) {
                    keyProperty = type
                        .GetProperties()
                        .FirstOrDefault(item => item.Name == $"{type.Name}Id");

                }
            }

            if (keyProperty == null) {
                throw new InvalidOperationException($"{type.FullName} has no Key property defined.");
            }

            return keyProperty;
        }

        public async Task<bool> SaveAsync(Model model)
        {
            foreach (var eventx in _events) {
                eventx.Saving(model);
            }

            // Find the item in the database.
            var id = GetPrimaryKey(model);
            bool isCreate = false;
            var dbModel = FindDbModelInDataById(id);
            if (dbModel == null) {
                isCreate = true;
                dbModel = new Model();
            }

            _modelConverter.CopyProperties(model, dbModel);

            if (isCreate) {
                _store.Data.Add(dbModel);
            } else {
                // dbModel has already updated _data.
            }

            await _store.SaveAsync();

            foreach (var eventx in _events) {
                eventx.Saved(model);
            }

            // Do nothing.
            return true;
        }

        public virtual async Task<bool> RemoveAsync(PrimaryKey id)
        {
            var dbModel = FindDbModelInDataById(id);
            _store.Data.Remove(dbModel);
            await _store.SaveAsync();

            if (_events.Any()) {
                var model = _modelConverter.CopyProperties(dbModel, new Model());

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

        public virtual Task<IEnumerable<Model>> FindAllAsync<TKey>(Expression<Func<Model, bool>> where = null, Expression<Func<Model, TKey>> orderBy = null, int skip = 0, int? take = null)
        {
            // Find the data set to work with.
            IEnumerable<Model> dbSet = _store.Data;

            // Apply the where clause.
            if (where != null) {
                dbSet = dbSet.Where(where.Compile());
            }

            // Apply order by clause.
            if (orderBy != null) {
                dbSet = dbSet.OrderBy(orderBy.Compile());
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
            var dbResults = dbSet.ToList();

            // Convert back to the model format.
            var ret = dbResults.Select(dbModel =>
            {
                var model = _modelConverter.CopyProperties(dbModel, new Model());

                foreach (var eventx in _events) {
                    eventx.Found(model);
                }

                return model;
            }).ToList();

            return Task.FromResult<IEnumerable<Model>>(ret);
        }

        public virtual Task<int> CountAsync(Expression<Func<Model, bool>> where = null, int skip = 0, int? take = null)
        {
            // Find the data set to work with.
            IEnumerable<Model> dbSet = _store.Data;

            // Apply the where clause.
            if (where != null) {
                dbSet = dbSet.Where(where.Compile());
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

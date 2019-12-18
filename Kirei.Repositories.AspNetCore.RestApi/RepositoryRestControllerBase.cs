using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Kirei.Repositories;

namespace Kirei.Repositories.AspNetCore.RestApi
{
    /// <summary>
    /// Base implementation of a REST API controller that uses a repository for all its operations.
    /// 
    /// This controller can be inherited to enable REST APIs for Models managed by a repository.
    /// </summary>
    /// <typeparam name="Model"></typeparam>
    public partial class RepositoryRestControllerBase<Model> : ControllerBase
        where Model : class, new()
    {
        private readonly IRepository<Model> _repository;

        public RepositoryRestControllerBase(
            IRepository<Model> repository
            )
        {
            _repository = repository;
        }

        /// <summary>
        /// Get all items.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Model> Get()
        {
            return _repository.FindAll();
        }

        /// <summary>
        /// Get an item.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public Model Get(Guid id)
        {
            return _repository.Find(id);
        }

        /// <summary>
        /// Post a new item.
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        public void Post([FromBody] Model value)
        {
            _repository.Save(value);
        }

        /// <summary>
        /// Update an existing item.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [HttpPut("{id}")]
        public void Put(Guid id, [FromBody] Model value)
        {
            _repository.Save(value);
        }

        /// <summary>
        /// Delete an an item.
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        public void Delete(Guid id)
        {
            _repository.Remove(id);
        }

        /// <summary>
        /// Return defaults for a new item.
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public Model Defaults()
        {
            return _repository.Create(Guid.NewGuid());
        }
    }
}

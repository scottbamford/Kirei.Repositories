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
        public async Task<IEnumerable<Model>> Get()
        {
            return await _repository.FindAllAsync();
        }

        /// <summary>
        /// Get an item.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<Model> Get(Guid id)
        {
            return await _repository.FindAsync(id);
        }

        /// <summary>
        /// Post a new item.
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        public async Task Post([FromBody] Model value)
        {
            await _repository.SaveAsync(value);
        }

        /// <summary>
        /// Update an existing item.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [HttpPut("{id}")]
        public async Task Put(Guid id, [FromBody] Model value)
        {
            await _repository.SaveAsync(value);
        }

        /// <summary>
        /// Delete an an item.
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        public async Task Delete(Guid id)
        {
            await _repository.RemoveAsync(id);
        }

        /// <summary>
        /// Return defaults for a new item.
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<Model> Defaults()
        {
            return await _repository.CreateAsync(Guid.NewGuid());
        }
    }
}

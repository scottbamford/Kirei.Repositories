using System;
using System.Collections.Generic;
using System.Text;

namespace Kirei.Repositories
{
    /// <summary>
    /// Concrete base class for implementating IRepositoryEvents&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RepositoryEvents<T> : IRepositoryEvents<T>
        where T : class
    {
        public virtual void Created(T model)
        {
        }

        public virtual void Found(T model)
        {
        }

        public virtual void Saving(T model)
        {
        }

        public virtual void Saved(T model)
        {
        }

        public virtual void Removed(T model)
        {
        }
    }
}

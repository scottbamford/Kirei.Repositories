using System;
using System.Collections.Generic;
using System.Text;

namespace Kirei.Repositories
{
    /// <summary>
    /// Events that get raised by IRepository to allow us to inject into the repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepositoryEvents<T> where T : class
    {
        void Created(T model);

        void Found(T model);

        void Saving(T model);

        void Saved(T model);

        void Removed(T model);

    }
}

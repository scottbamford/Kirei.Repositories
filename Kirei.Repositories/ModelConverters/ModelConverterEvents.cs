using System;
using System.Collections.Generic;
using System.Text;

namespace Kirei.Repositories
{
    /// <summary>
    /// Base implementation of IModelConverterEvents that can be used as a subclass.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ModelConverterEvents<A, B> : IModelConverterEvents<A, B>
    {
        public virtual void Converting(A a, B b)
        {
        }

        public virtual void Converted(A a, B b)
        {
        }
    }
}

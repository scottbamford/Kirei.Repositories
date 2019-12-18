using System;
using System.Collections.Generic;
using System.Text;

namespace Kirei.Repositories
{
    /// <summary>
    /// Events that get raised by IModelConverter to allow us to inject into the conversion process for specific types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IModelConverterEvents<A, B>
    {

        /// <summary>
        /// <paramref name="a"/> is about to be converted to <paramref name="b"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        void Converting(A a, B b);

        /// <summary>
        /// <paramref name="a"/> has been converted to <paramref name="b"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        void Converted(A a, B b);
    }
}

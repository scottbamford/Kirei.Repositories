using System;
using System.Collections.Generic;
using System.Text;

namespace Kirei.Repositories
{
    /// <summary>
    /// Class that converts between two types.
    /// </summary>
    public interface IModelConverter
    {
        /// <summary>
        /// Copy all properties from <paramref name="from"/> into <paramref name="to"/> based on matching names.
        /// </summary>
        /// <returns></returns>
        To CopyProperties<From, To>(From from, To to);

        /// <summary>
        /// Convert <paramref name="from"/> into a new instance of <typeparamref name="To"/> copying across all properties with CopyProperties().
        /// </summary>
        /// <typeparam name="From"></typeparam>
        /// <typeparam name="To"></typeparam>
        /// <param name="from"></param>
        /// <returns></returns>
        To Convert<From, To>(From from)
            where To : new();


        /// <summary>
        /// Convert <paramref name="from"/> into a new instance of <typeparamref name="To"/> copying across all properties with CopyProperties().
        /// </summary>
        /// <typeparam name="To"></typeparam>
        /// <param name="from"></param>
        /// <returns></returns>
        To Convert<To>(object from)
            where To : new();
    }
}

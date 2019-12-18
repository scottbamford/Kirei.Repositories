using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Kirei.Repositories
{
    /// <summary>
    /// Base implementation of IModelConverter that uses reflection to copy properties.
    /// </summary>
    public class ModelConverter : IModelConverter
    {
        private readonly IServiceProvider _serviceProvider;

        public ModelConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Copy properties from <paramref name="from"/> into <paramref name="to"/>.
        /// </summary>
        /// <typeparam name="From"></typeparam>
        /// <typeparam name="To"></typeparam>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public virtual To CopyProperties<From, To>(From from, To to)
        {
            var events = _serviceProvider.GetServices<ModelConverterEvents<From, To>>();

            // Raise the converting event.
            foreach (var eventx in events) {
                eventx.Converting(from, to);
            }

            if (from == null || to == null) {
                return to;
            }

            var fromType = typeof(From) == typeof(object) ? from.GetType() : typeof(From);
            var fromProperties = fromType.GetProperties();
            var toProperties = typeof(To).GetProperties();
            foreach (var fromProperty in fromProperties) {
                if (!fromProperty.CanRead) {
                    continue;
                }

                var toProperty = toProperties.FirstOrDefault(item => item.Name == fromProperty.Name);
                if (toProperty == null || !toProperty.CanWrite) {
                    continue;
                }

                if (toProperty.PropertyType != fromProperty.PropertyType) {
                    continue;
                }

                toProperty.SetValue(to, fromProperty.GetValue(from));
            }

            // Raise the converted event.
            foreach (var eventx in events) {
                eventx.Converted(from, to);
            }

            return to;
        }



        /// <summary>
        /// </summary>
        /// <typeparam name="From"></typeparam>
        /// <typeparam name="To"></typeparam>
        /// <param name="from"></param>
        /// <returns></returns>
        public virtual To Convert<From, To>(From from)
            where To : new()
        {
            return CopyProperties<From, To>(from, new To());
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="From"></typeparam>
        /// <typeparam name="To"></typeparam>
        /// <param name="from"></param>
        /// <returns></returns>
        public virtual To Convert<To>(object from)
            where To : new()
        {
            return Convert<object, To>(from);
        }
    }
}

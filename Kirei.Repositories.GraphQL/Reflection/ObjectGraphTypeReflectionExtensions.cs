using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL.Utilities;
using GraphQL.Types;

namespace Kirei.Repositories.GraphQL
{

    /// <summary>
    /// Extension methods that utalise reflection to allow you to reduce the amount of code you use when creating GraphObjectTypes.
    /// </summary>
    public static class ObjectGraphTypeReflectionExtensions
    {
        /// <summary>
        /// Add all reflected properties as fields (unless they are specified in <paramref name="excludedProperties"/>.
        /// </summary>
        /// <typeparam name="TSourceType"></typeparam>
        /// <param name="type"></param>
        /// <param name="excludedProperties"></param>
        public static void AddAllPropertiesAsFields<TSourceType>(this ComplexGraphType<TSourceType> type, params Expression<Func<TSourceType, object>>[] excludedProperties)
        {
            GraphObjectTypeReflectionHelper.SetFields(
                type,
                GetRegisteredProperties(typeof(TSourceType)),
                new GraphObjectTypeReflectionOptions<TSourceType>
                {
                    ExcludedProperties = excludedProperties,
                }
                );
        }

        /// <summary>
        /// Add all reflected properties as fields (unless they are specified in <paramref name="excludedProperties"/>.
        /// </summary>
        /// <typeparam name="TSourceType"></typeparam>
        /// <param name="type"></param>
        /// <param name="nullable"></param>
        /// <param name="excludedProperties"></param>
        public static void AddAllPropertiesAsFields<TSourceType>(this ComplexGraphType<TSourceType> type, bool? nullable = null, params Expression<Func<TSourceType, object>>[] excludedProperties)
        {
            GraphObjectTypeReflectionHelper.SetFields(
                type,
                GetRegisteredProperties(typeof(TSourceType)),
                new GraphObjectTypeReflectionOptions<TSourceType>
                {
                    Nullable = nullable,
                    ExcludedProperties = excludedProperties,
                }
                );
        }

        /// <summary>
        /// Add all reflected properties as fields (unless they are specified in <paramref name="excludedProperties"/>.
        /// 
        /// Properties with names already registered will be excluded automatically, allowing you to do this after first specifying those fields you want to control manually.
        /// </summary>
        /// <typeparam name="TSourceType"></typeparam>
        /// <param name="type"></param>
        /// <param name="excludedProperties"></param>
        public static void AddRemainingPropertiesAsFields<TSourceType>(this ComplexGraphType<TSourceType> type, params Expression<Func<TSourceType, object>>[] excludedProperties)
        {
            GraphObjectTypeReflectionHelper.SetFields(
                type,
                GetRegisteredProperties(typeof(TSourceType)),
                new GraphObjectTypeReflectionOptions<TSourceType>
                {
                    ExcludedProperties = excludedProperties,
                    SkipExisting = true,
                }
                );
        }

        /// <summary>
        /// Add all reflected properties as fields (unless they are specified in <paramref name="excludedProperties"/>.
        /// 
        /// Properties with names already registered will be excluded automatically, allowing you to do this after first specifying those fields you want to control manually.
        /// </summary>
        /// <typeparam name="TSourceType"></typeparam>
        /// <param name="type"></param>
        /// <param name="nullable"></param>
        /// <param name="excludedProperties"></param>
        public static void AddRemainingPropertiesAsFields<TSourceType>(this ComplexGraphType<TSourceType> type, bool? nullable = null, params Expression<Func<TSourceType, object>>[] excludedProperties)
        {
            GraphObjectTypeReflectionHelper.SetFields(
                type,
                GetRegisteredProperties(typeof(TSourceType)),
                new GraphObjectTypeReflectionOptions<TSourceType>
                {
                    Nullable = nullable,
                    ExcludedProperties = excludedProperties,
                    SkipExisting = true,
                }
                );
        }

        private static IEnumerable<PropertyInfo> GetRegisteredProperties(Type sourceType)
        {
            return sourceType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => GraphObjectTypeReflectionHelper.IsEnabledForRegister(p.PropertyType, true));
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL;
using GraphQL.Utilities;
using GraphQL.Types;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Options that can be passed to the GraphObjectTypeReflectionHelper and classes that use it.
    /// </summary>
    public class GraphObjectTypeReflectionOptions<TSourceType>
    {
        public Expression<Func<TSourceType, object>>[] ExcludedProperties { get; set; }
        public Expression<Func<TSourceType, object>>[] NullableProperties { get; set; }
        public bool? Nullable { get; set; }
        public Expression<Func<TSourceType, object>>[] NonNullableProperties { get; set; }
        public bool SkipExisting { get; set; }
    }

    /// <summary>
    /// Helper class containing the logic used by ExtendedAutoRegistrationGraphType and friends as well as the extension methods frmo ObjectGraphTypeAutoRegistrationExtensions.
    /// </summary>
    public static class GraphObjectTypeReflectionHelper
    {
        public static void SetFields<TSourceType>(ComplexGraphType<TSourceType> type, IEnumerable<PropertyInfo> properties, GraphObjectTypeReflectionOptions<TSourceType> options)
        {
            // Default the options if we get a null passed in.
            if (options == null) {
                options = new GraphObjectTypeReflectionOptions<TSourceType>();
            }

            //type.Name = typeof(TSourceType).GraphQLName(); // Don't do this, it will then break with Input types.

            // Helper method that works out if 
            bool isNullable(PropertyInfo propertyInfo)
            {
                if (!options.Nullable.HasValue) {
                    return IsNullableProperty(propertyInfo);
                }

                if (options.NullableProperties?.Any(p => GetPropertyName(p) == propertyInfo.Name) == true) {
                    return true;
                }

                if (options.NonNullableProperties?.Any(p => GetPropertyName(p) == propertyInfo.Name) == true) {
                    return false;
                }

                return options.Nullable.Value;
            }

            foreach (var propertyInfo in properties) {
                if (options.ExcludedProperties?.Any(p => GetPropertyName(p) == propertyInfo.Name) == true) {
                    continue;
                }

                if (options.SkipExisting) {
                    if (type.Fields.Any(p => p.Name == propertyInfo.Name)) {
                        continue;
                    }
                }

                type.Field(
                    type: propertyInfo.PropertyType.GetGraphTypeFromType(isNullable(propertyInfo)),
                    name: propertyInfo.Name,
                    description: propertyInfo.Description(),
                    deprecationReason: propertyInfo.ObsoleteMessage()
                ).DefaultValue = (propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() as DefaultValueAttribute)?.Value;
            }
        }

        private static bool IsNullableProperty(PropertyInfo propertyInfo)
        {
            if (Attribute.IsDefined(propertyInfo, typeof(RequiredAttribute))) return false;

            // Strings default to false.
            if (propertyInfo.PropertyType == typeof(string)) {
                return false;
            }

            if (!propertyInfo.PropertyType.IsValueType) return true;

            return propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static string GetPropertyName<TSourceType>(Expression<Func<TSourceType, object>> expression)
        {
            if (expression.Body is MemberExpression m1)
                return m1.Member.Name;

            if (expression.Body is UnaryExpression u && u.Operand is MemberExpression m2)
                return m2.Member.Name;

            throw new NotSupportedException($"Unsupported type of expression: {expression.GetType().Name}");
        }

        public static bool IsEnabledForRegister(Type propertyType, bool firstCall)
        {
            if (propertyType == typeof(string)) return true;

            if (propertyType.IsValueType) return true; // TODO: requires discussion: Nullable<T>, enums, any struct

            //if (GraphTypeTypeRegistry.Contains(propertyType)) return true;

            if (firstCall) {
                var realType = GetRealType(propertyType);
                if (realType != propertyType)
                    return IsEnabledForRegister(realType, false);
            }

            return false;
        }

        private static Type GetRealType(Type propertyType)
        {
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                return propertyType.GetGenericArguments()[0];
            }

            if (propertyType.IsArray) {
                return propertyType.GetElementType();
            }

            if (propertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyType)) {
                return propertyType.GetGenericArguments()[0];
            }

            return propertyType;
        }
    }
}
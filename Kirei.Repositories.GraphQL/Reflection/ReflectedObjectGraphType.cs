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
    /// Reflecition based autoamtic InputObjectGraphType generation.
    /// 
    /// This class is based on GraphQL.Types.AutoRegistrationInputObjectGraphType but has more options and used GraphTypeObjectTypeReflectionHelper.
    /// 
    /// Allows you to automatically register the necessary fields for the specified type.
    /// Supports <see cref="DescriptionAttribute"/>, <see cref="ObsoleteAttribute"/>, <see cref="DefaultValueAttribute"/> and <see cref="RequiredAttribute"/>.
    /// Also it can get descriptions for fields from the xml comments.
    /// </summary>
    /// <typeparam name="TSourceType"></typeparam>
    public class ReflectedObjectGraphType<TSourceType> : ObjectGraphType<TSourceType>
    {
        /// <summary>
        /// Creates a GraphQL type from <typeparamref name="TSourceType"/>.
        /// </summary>
        public ReflectedObjectGraphType() : this(null) { }

        /// <summary>
        /// Creates a GraphQL type from <typeparamref name="TSourceType"/> by specifying additional options.
        /// </summary>
        public ReflectedObjectGraphType(GraphObjectTypeReflectionOptions<TSourceType> options)
        {
            GraphObjectTypeReflectionHelper.SetFields(this, GetRegisteredProperties(), options);
        }

        protected virtual IEnumerable<PropertyInfo> GetRegisteredProperties()
        {
            return typeof(TSourceType)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => GraphObjectTypeReflectionHelper.IsEnabledForRegister(p.PropertyType, true));
        }
    }
}
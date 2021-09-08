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

namespace Kirei.Repositories.GraphQL.Reflection
{
    /// <summary>
    /// Reflecition based autoamtic InputObjectGraphType generation.
    /// 
    /// This class is based on GraphQL.Types.AutoRegistrationInputObjectGraphType but has more options and used GraphTypeObjectTypeReflectionHelper.
    /// 
    /// Allows you to automatically register the necessary fields for the specified input type.
    /// Supports <see cref="DescriptionAttribute"/>, <see cref="ObsoleteAttribute"/>, <see cref="DefaultValueAttribute"/> and <see cref="RequiredAttribute"/>.
    /// Also it can get descriptions for fields from the xml comments.
    /// Note that now __InputValue has no isDeprecated and deprecationReason fields but in the future they may appear - https://github.com/graphql/graphql-spec/pull/525
    /// </summary>
    /// <typeparam name="TSourceType"></typeparam>
    public class ReflectedInputObjectGraphType<TSourceType> : InputObjectGraphType<TSourceType>
    {
        /// <summary>
        /// Creates a GraphQL type from <typeparamref name="TSourceType"/>.
        /// </summary>
        public ReflectedInputObjectGraphType() : this(null) { }

        /// <summary>
        /// Creates a GraphQL type from <typeparamref name="TSourceType"/> by specifying additional options.
        /// </summary>
        public ReflectedInputObjectGraphType(GraphObjectTypeReflectionOptions<TSourceType> options)
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Utility methods for converting values between complex and simple types.
    /// </summary>
    public static class ConversionUtilities
    {
        /// <summary>
        /// Convert <paramref name="value"/> into <paramref name="targetType"/> either directy or via ToString() and Parse()
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object ConvertToType(object value, Type targetType)
        {
            // Null is always null.
            if (value == null) {
                return null;
            }

            // If the framework can convert on assignment it then let it do so.
            if (targetType.IsAssignableFrom(value.GetType())) {
                return value;
            }

            // Attempt to convert nullable types into their non-nullable format.
            if (value is Nullable && Nullable.GetUnderlyingType(value.GetType()) == targetType) {
                return value;
            }

            // We can turn anything into a string.
            if (targetType == typeof(string)) {
                return value.ToString();
            }

            // Try and parse it
            var parseMethod = targetType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(item => item.Name == "Parse")
                .Where(item => item.GetParameters().Length >= 1 && item.GetParameters().First().ParameterType == typeof(string))
                .OrderBy(item => item.GetParameters().Length)
                .FirstOrDefault();

            var sValue = value.ToString();
            try {
                var parsedValue = parseMethod.Invoke(null, new object[] { sValue });
                return parsedValue;
            } catch (Exception) {
                // Ignore, it just lets us know the Parse failed.
            }

            // Give up and let the calling code fail on assignment.
            return value;
        }

        /// <summary>
        /// Apply <paramref name="changes"/> to <paramref name="model"/> matching by name (case insenstive) and converting the type if required.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="changes"></param>
        public static void ApplyChanges<Model>(Model model, object changes)
        {
            // Build a dictionary of changes to be applied.
            IDictionary<string, object> changesDictionary;
            if (changes is IDictionary<string, object>) {
                changesDictionary = (IDictionary<string, object>)changes;
            } else {
                // Treat changes as an object and use reflection to build a list of changes.
                changesDictionary = changes.GetType().GetProperties()
                    .Where(item => item.CanRead)
                    .ToDictionary(item => item.Name, item => item.GetValue(changes));
            }

            foreach (var change in changesDictionary) {
                var property = FindProperty(model, change.Key);
                if (property == null) {
                    throw new ArgumentException($"Model does not contain a property called \"{change.Key}\" that has been included in the changes to be applied.", "changes");
                }

                property.SetValue(model, ConvertToType(change.Value, property.PropertyType));
            }
        }

        /// <summary>
        /// Returns a property for <paramref name="name"/> on <paramref name="obj"/> or null if no property could be found.
        /// </summary>
        /// <remarks>
        /// When matching a property <paramref name="name"/> is treated case insensitive, however if there is more than once match, a case sensitive match is always preferred.
        /// </remarks>
        /// <returns></returns>
        public static System.Reflection.PropertyInfo FindProperty(object obj, string name)
        {
            var type = obj.GetType();
            var property = type.GetProperty(name);
            if (property == null) {
                property = type.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            }

            return property;
        }
    }
}

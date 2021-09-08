using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Dynamic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Object that supports use as dynamic, which is case insenstive with its properties.
    /// </summary>
    public class CaseInsenstiveDynamicDictionary : DynamicObject, IDictionary<string, object>
    {
        /// <summary>
        /// Readonly dictionary that is case insenstive.  Used to store all our properties.
        /// </summary>
        private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        public CaseInsenstiveDynamicDictionary()
        {
        }

        public CaseInsenstiveDynamicDictionary(IDictionary<string, object> copyFrom)
        {
            foreach (var item in copyFrom) {
                _dictionary.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Get or set an item in the case-insenstive dictionary (this is the primary API.)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key] { get => _dictionary[key]; set => _dictionary[key] = value; }

        /// <summary>
        /// Returns this object as a dynamic.
        /// </summary>
        public dynamic AsDynamic() => this;

        #region DynamicObject implementation
        /// <summary>
        /// If you try get a property of a dynamic object that is not defined in the class this method is called.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            return _dictionary.TryGetValue(binder.Name, out result);
        }

        /// <summary>
        /// If you try set a property of a dynamic object that is not defined in the class this method is called.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            _dictionary[binder.Name.ToLower()] = value;

            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }
        #endregion

        #region Explicit implementation of IDictionary<string, object>

        ICollection<string> IDictionary<string, object>.Keys => ((IDictionary<string, object>)_dictionary).Keys;

        ICollection<object> IDictionary<string, object>.Values => ((IDictionary<string, object>)_dictionary).Values;

        int ICollection<KeyValuePair<string, object>>.Count => throw new NotImplementedException();

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => throw new NotImplementedException();

        void IDictionary<string, object>.Add(string key, object value)
        {
            _dictionary.Add(key, value);
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, object>)_dictionary).GetEnumerator();
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            _dictionary.Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            _dictionary.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return _dictionary.ContainsKey(item.Key) && _dictionary[item.Key] == item.Value;
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach (var item in _dictionary) {
                array[i++] = item;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return _dictionary.Remove(item.Key);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// InputObjectGraphType subclass that overrides ParseDictionary to return a InputModelDictionary object rather than just the
    /// parsed model so we can preserve the dictionary as it was received as well as the parsed result.
    /// 
    /// This is useful when you want to only update changed fields in a model in a database for example.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class InputModelDictionaryObjectGraphType<TModel> : InputObjectGraphType<TModel>
    {
        public InputModelDictionaryObjectGraphType()
            :base()
        {
        }

        /// <summary>
        /// Override the parsing process to preserve both the parsed model (standard behaviour) and the dictionary/changes actually received from the mutation.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object ParseDictionary(IDictionary<string, object> value)
        {
            var model = (TModel)base.ParseDictionary(value);
            return new InputModelDictionary<TModel>(model, value);
        }
    }
}

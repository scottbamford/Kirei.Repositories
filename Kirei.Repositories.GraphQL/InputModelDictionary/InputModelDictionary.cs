using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kirei.Repositories.GraphQL
{
    /// <summary>
    /// Class that exposes the parsed model of TModel, the case insensitive dictioary of changes, and the raw Dictionary received as an input from
    /// a GraphQL.
    /// 
    /// To use either include this code in your InputObjectGraphType&lt;TModel&gt; subclass:
    /// 
    /// public override object ParseDictionary(IDictionary<string, object> value)
    /// {
    ///     var model = (TModel)base.ParseDictionary(value);
    ///     return new InputModelDictionary<TModel>(model, value);
    /// }
    /// 
    /// Or inherit from InputModelDictionaryObjectGraphType&lt;TModel&gt; to have it done for you automatically.
    /// 
    /// To use:
    ///     var input = context.GetArgument<InputModelDictionary<Models.QuestionTag>>("model");
    ///     input["FieldName"] // Use the input as a insenstive dictionary (CaseInsenstiveDynamicDictionary).
    ///     input.GetModel() // Returns the model.
    ///     input.GetRawDictionary() // Returns the raw dictionary.
    ///     
    /// You can also cast to get the model:
    ///     var model = (TModel)input;
    /// </summary>
    public class InputModelDictionary<TModel> : CaseInsenstiveDynamicDictionary
    {
        private readonly TModel _model;
        private readonly IDictionary<string, object> _rawDictionary;

        public InputModelDictionary(TModel model, IDictionary<string, object> dictionary)
            : base(dictionary)
        {
            _model = model;
            _rawDictionary = dictionary;
        }

        /// <summary>
        /// Returns the raw dictionary received by the GraphQL query (contents will mirror those in Changes but without any case insensitive or dynamic support).
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> GetRawDictionary()
        {
            return _rawDictionary;
        }

        /// <summary>
        /// Returns the prased model.
        /// </summary>
        public TModel GetModel()
        {
            return _model;
        }

        /// <summary>
        /// Allow explicit casts into the model type.
        /// </summary>
        /// <param name="model"></param>
        public static explicit operator TModel(InputModelDictionary<TModel> input)
        {
            return input._model;
        }
    }
}

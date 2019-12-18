using System;
using System.Collections.Generic;
using System.Text;

namespace Kirei.Repositories
{
    /// <summary>
    /// Options for JsonFileRepositoryStore that can be used with IOptions.
    /// </summary>
    public class JsonFileRepositoryStoreOptions
    {
        public JsonFileRepositoryStoreOptions()
        {
            ResourcesPath = "Resources";
        }

        /// <summary>
        /// Path for the json files.  Default is Resources.
        /// </summary>
        public string ResourcesPath { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;

namespace Kirei.Repositories
{
    /// <summary>
    /// IMemoryRepositoryStore that persists in Json files between sessions.
    /// </summary>
    /// <remarks>
    /// Use IOptions&lgt;JsonFileRepositoryStoreOptions&gt; to configure where the Json files are stored (default is Resources).
    /// </remarks>
    public class JsonFileRepositoryStore<T> : IRepositoryStore<T>
    {
        private readonly IFileProvider _fileProvider;
        private readonly string _resourcesContainer;
        private readonly IList<T> _data;
        private bool _dataLoaded = false;

        public JsonFileRepositoryStore(
            IHostingEnvironment hostingEnvironment,
            IOptions<JsonFileRepositoryStoreOptions> options
            )
        {
            _fileProvider = hostingEnvironment.ContentRootFileProvider;
            _resourcesContainer = options.Value.ResourcesPath;

            _data = new List<T>();
        }

        protected bool Load()
        {
            return LoadAsync().Result;
        }

        protected async Task<bool> LoadAsync()
        {
            string modelName = typeof(T).Name;

            foreach (var fileInfo in GetLocations(modelName)) {
                var loadedData = await LoadFileAsync(fileInfo);
                if (loadedData != null) {
                    foreach (var item in loadedData) {
                        _data.Add(item);
                    }
                }
            }

            return true;
        }

        public Task<bool> SaveAsync()
        {
            var json = JsonSerializer.Serialize(_data);

            string modelName = typeof(T).Name;

            var fileInfo = GetLocations(modelName).First();
            if (String.IsNullOrEmpty(fileInfo?.PhysicalPath)) {
                throw new Exception("Unable to find a suitable location to save the data");
            }

            File.WriteAllText(fileInfo.PhysicalPath, json);

            return Task.FromResult(true);
        }
        
        public IList<T> Data
        {
            get
            {
                if (!_dataLoaded) {
                    _dataLoaded = true;
                    Load();
                }

                return _data;
            }
        }

        protected IEnumerable<IFileInfo> GetLocations(string modelName)
        {
            yield return _fileProvider.GetFileInfo(Path.Combine(_resourcesContainer, modelName + ".json"));
        }
        
        protected async Task<IEnumerable<T>> LoadFileAsync(IFileInfo fileInfo)
        {
            if (!fileInfo.Exists) {
                return null;
            }

            using (var stream = fileInfo.CreateReadStream()) {
                using (var reader = new StreamReader(stream)) {
                    var json = await reader.ReadToEndAsync();
                    var ret = JsonSerializer.Deserialize<T[]>(json);
                    return ret;
                }
            }

            /* NOTREACHED */
        }
    }
}

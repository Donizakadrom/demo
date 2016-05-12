using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Api.HttpControllerSelectors.Base
{
    public abstract class VersionControllerSelectorBase: DefaultHttpControllerSelector
    {
        private readonly HttpConfiguration config;
        private readonly Lazy<Dictionary<string, HttpControllerDescriptor>> _controllerDescriptors; 

        protected VersionControllerSelectorBase(HttpConfiguration configuration) : base(configuration)
        {
            config = configuration;
            _controllerDescriptors = new Lazy<Dictionary<string, HttpControllerDescriptor>>(InitializeControllerDescriptors);
        }

        private Dictionary<string, HttpControllerDescriptor> InitializeControllerDescriptors()
        {
            var dictionary = new Dictionary<string, HttpControllerDescriptor>(StringComparer.OrdinalIgnoreCase);

            var assembliesResolver = config.Services.GetAssembliesResolver();
            var controllersResolver = config.Services.GetHttpControllerTypeResolver();
            var controllerTypes = controllersResolver.GetControllerTypes(assembliesResolver);
            var duplicates = new List<string>();

            foreach (var controllerType in controllerTypes)
            {
                // For the dictionary key, strip "Controller" from the end of the type name.
                // This matches the behavior of DefaultHttpControllerSelector.
                var controllerName = controllerType.Name.Remove(controllerType.Name.Length - DefaultHttpControllerSelector.ControllerSuffix.Length);

                var key = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", controllerType.Assembly.GetName().Name, controllerName);

                // Check for duplicate keys.
                if (dictionary.Keys.Contains(key))
                {
                    if (!duplicates.Contains(key))
                    {
                        duplicates.Add(key);
                    }
                }
                else
                {
                    dictionary[key] = new HttpControllerDescriptor(config, controllerName, controllerType);
                }
            }

            foreach (var duplicate in duplicates)
            {
                dictionary.Remove(duplicate);
            }

            return dictionary;
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            object versionObj;
            var version = GetApiVersion(request);

            if (!version.HasValue)
            {
                return null;
            }

            var controllerName = GetControllerName(request);
            var type = GetControllerType(version.Value, controllerName);
            return type == null ? null : new HttpControllerDescriptor(config, controllerName, type);
        }

        protected abstract int? GetApiVersion(HttpRequestMessage request);

        private Type GetControllerType(int version, string controllerName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var currVersionAssembly = assemblies.FirstOrDefault(x => string.Equals(x.GetName().Name.ToLower(), "api.v" + version, StringComparison.InvariantCultureIgnoreCase));

            var type =
                currVersionAssembly != null ? currVersionAssembly.GetTypes()
                    .FirstOrDefault(
                        x =>
                            !x.IsAbstract && typeof (IHttpController).IsAssignableFrom(x) &&
                            string.Equals(x.Name, string.Format("{0}{1}", controllerName, ControllerSuffix),
                                StringComparison.InvariantCultureIgnoreCase)) : null;

            return type;
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _controllerDescriptors.Value;
        }
    }
}
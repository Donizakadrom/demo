using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Http.Dependencies;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using Unity.WebApi;

namespace Api.DependencyResolvers
{
    public class ApiUrlVersionDependencyResolver: IDependencyResolver
    {
        private readonly IUnityContainer _container;
        private readonly IDependencyResolver _fallbackResolver;

        public ApiUrlVersionDependencyResolver(IUnityContainer container, IDependencyResolver fallbackResolver)
        {
            _container = container;
            _fallbackResolver = fallbackResolver;
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        public object GetService(Type serviceType)
        {
            var container = GetApiVersionContainer() ?? _container;

            try
            {
                return container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return _fallbackResolver.GetService(serviceType);
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var container = GetApiVersionContainer() ?? _container;

            return container.ResolveAll(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            return new UnityDependencyResolver(GetApiVersionContainer() ?? _container);
        }

        private IUnityContainer GetApiVersionContainer()
        {
            int? version = null;

            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request.RequestContext != null)
                {
                    version = GetApiVersion(HttpContext.Current.Request.RequestContext);
                }
            }
            catch (HttpException)
            {
            }

            return version.HasValue ? _container.Resolve<IUnityContainer>(version.Value.ToString()) : null;
        }

        protected int? GetApiVersion(RequestContext request)
        {
            object versionObj;
            var success = request.RouteData.Values.TryGetValue("version", out versionObj);

            if (!success)
            {
                return null;
            }

            return int.Parse((string)versionObj);
        }
    }
}
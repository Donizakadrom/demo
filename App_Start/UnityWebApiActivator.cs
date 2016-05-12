using System.Web.Http;
using System.Web.Mvc;
using Api;
using Api.DependencyResolvers;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.WebApi;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(UnityWebApiActivator), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(UnityWebApiActivator), "Shutdown")]

namespace Api
{
    /// <summary>Provides the bootstrapping for integrating Unity with WebApi when it is hosted in ASP.NET</summary>
    public static class UnityWebApiActivator
    {
        /// <summary>Integrates Unity when the application starts.</summary>
        public static void Start() 
        {
            // Use UnityHierarchicalDependencyResolver if you want to use a new child container for each IHttpController resolution.
            // var resolver = new UnityHierarchicalDependencyResolver(UnityConfig.GetConfiguredContainer());

            IUnityContainer container = UnityConfig.GetConfiguredContainer();

            GlobalConfiguration.Configuration.DependencyResolver = new ApiUrlVersionDependencyResolver(container, GlobalConfiguration.Configuration.DependencyResolver);

            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));
        }

        /// <summary>Disposes the Unity container when the application is shut down.</summary>
        public static void Shutdown()
        {
            var container = UnityConfig.GetConfiguredContainer();
            container.Dispose();
        }
    }
}

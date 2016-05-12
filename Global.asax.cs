using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Meerkat.Core.Web;
using Microsoft.Practices.Unity;
using TourProvider.Common.Repository.Interfaces;

namespace Api
{
    public class WebApiApplication : HttpApplication
    {
        protected async void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            MapsConfig.Register();

            IUnityContainer container = UnityConfig.GetConfiguredContainer();
            ITourProviderRepository tourProviderRepository = container.Resolve<ITourProviderRepository>();
            await tourProviderRepository.Initialize(false);
        }
    }
}
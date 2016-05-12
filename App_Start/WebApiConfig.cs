using System.Data.Entity;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using Api.Filters;
using Api.Handlers;
using Api.HttpControllerSelectors;
using DAL;
using DAL.Migrations;
using Microsoft.Owin.Security.OAuth;

namespace Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            config.MessageHandlers.Add(new ApiLogHandler());

            config.Filters.Add(new ApiExceptionFilter());

            config.MapHttpAttributeRoutes();
            config.Services.Replace(typeof(IHttpControllerSelector), new ApiUrlVersionControllerSelector(GlobalConfiguration.Configuration));
            config.Routes.MapHttpRoute("DefaultGetAll", "v{version}/{controller}", new { id = UrlParameter.Optional, action = "GetAll" }, new { version = @"^[0-9]{1,2}$" });
            config.Routes.MapHttpRoute("DefaultGet", "v{version}/{controller}/id{id}", new { id = UrlParameter.Optional, action = "Get" }, new { version = @"^[0-9]{1,2}$" });
            config.Routes.MapHttpRoute("Default", "v{version}/{controller}/{action}", new {}, new { version=@"^[0-9]{1,2}$"});

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<EFContext, Configuration>());
        }
    }
}
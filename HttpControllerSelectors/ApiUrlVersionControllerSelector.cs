using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using Api.HttpControllerSelectors.Base;

namespace Api.HttpControllerSelectors
{
    public class ApiUrlVersionControllerSelector: VersionControllerSelectorBase
    {
        public ApiUrlVersionControllerSelector(HttpConfiguration configuration) : base(configuration)
        {
        }

        protected override int? GetApiVersion(HttpRequestMessage request)
        {
            object versionObj;
            var success = request.GetRouteData().Values.TryGetValue("version", out versionObj);

            if (!success)
            {
                return null;
            }

            return int.Parse((string)versionObj);
        }
    }
}
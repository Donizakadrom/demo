using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Api.HttpControllerSelectors.Base;

namespace Api.HttpControllerSelectors
{
    public class ApiHeaderVersionControllerSelector: VersionControllerSelectorBase
    {
        public ApiHeaderVersionControllerSelector(HttpConfiguration configuration) : base(configuration)
        {
        }

        protected override int? GetApiVersion(HttpRequestMessage request)
        {
            var value = request.Headers.GetValues("ApiVersion").FirstOrDefault();

            return value == null ? null : (int?) int.Parse(value);
        }
    }
}
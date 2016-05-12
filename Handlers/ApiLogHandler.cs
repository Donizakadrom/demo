// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiLogHandler.cs" company="Ingenius Systems">
//   Copyright (c) Ingenius Systems
//   Create on 09.11.2015 10:32:30 by Albert Zakiev
// </copyright>
// <summary>
//   Defines the ApiLogHandler type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;

namespace Api.Handlers
{
    /// <summary>
    /// API request logger
    /// </summary>
    public class ApiLogHandler : DelegatingHandler
    {
        #region Private fields

        private static readonly Logger Logger = LogManager.GetLogger("ApiLogger");

        #endregion

        #region Protected methods

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await LogRequest(request);

            return await base.SendAsync(request, cancellationToken);
        }

        #endregion

        #region Private methods

        private async Task LogRequest(HttpRequestMessage request)
        {
            try
            {
                Logger.Info("Request - {0}:{1}", request.Method, request.RequestUri);
                
                if (request.Content != null)
                {
                    string requestContent = await request.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(requestContent))
                    {
                        string formattedRequest = GetFormattedJson(requestContent);
                        Logger.Info("{0}{1}", Environment.NewLine, formattedRequest);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private string GetFormattedJson(string content)
        {
            try
            {
                return JObject.Parse(content).ToString();
            }
            catch
            {
                return content;
            }
        }
        
        #endregion
    }
}
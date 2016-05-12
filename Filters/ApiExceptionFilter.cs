// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiExceptionFilter.cs" company="Ingenius Systems">
//   Copyright (c) Ingenius Systems
//   Create on 23.10.2015 11:17:51 by Albert Zakiev
// </copyright>
// <summary>
//   Defines the ApiExceptionFilter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using Api.Common.Legacy.v3.Models.Base;
using NLog;
using Tools.Exceptions;
using TourProvider.Common.Exceptions;

namespace Api.Filters
{
    /// <summary>
    /// Фильтр ошибок Web API
    /// </summary>
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        #region Private fields

        private readonly Logger _logger = LogManager.GetLogger("ApiLogger");

        #endregion

        #region Public methods

        /// <summary>
        /// Raises the exception event.
        /// </summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response == null)
            {
                _logger.Error(actionExecutedContext.Exception);

                Exception exception = actionExecutedContext.Exception;

                HttpException httpException = exception as HttpException;
                if (httpException != null)
                {
                    actionExecutedContext.Response = CreateErrorResponse(actionExecutedContext,
                        (HttpStatusCode) httpException.GetHttpCode(), httpException.Message);
                }
                else
                {
                    var exceptionMessage = GetDeepException(exception).Message;

                    if (IsTourProviderException(exception))
                    {
                        exceptionMessage = string.Format("{0}{1}{2}", "Tour Provider API Error", Environment.NewLine,
                            exceptionMessage);
                    }

                    actionExecutedContext.Response = CreateErrorResponse(actionExecutedContext, HttpStatusCode.OK,
                        exceptionMessage);
                }
            }
        }

        #endregion

        #region Private methods

        private Exception GetDeepException(Exception ex)
        {
            return ex.InnerException == null ? ex : GetDeepException(ex.InnerException);
        }

        private bool IsTourProviderException(Exception ex)
        {
            return ex.GetType() == typeof (TourProviderModuleException) ||
                   (ex.InnerException != null && IsTourProviderException(ex.InnerException));
        }

        private HttpResponseMessage CreateErrorResponse(HttpActionExecutedContext actionExecutedContext, HttpStatusCode httpCode, string message)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>
            {
                Success = false,
                Model = message
            };

            return actionExecutedContext.Request.CreateResponse(httpCode, responseModel);
        }

        #endregion
    }
}
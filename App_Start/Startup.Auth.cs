// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartupAuth.cs" company="Ingenius Systems">
//   Copyright (c) Ingenius Systems
//   Create on 28.10.2015 11:24:48 by Albert Zakiev
// </copyright>
// <summary>
//   Defines the StartupAuth type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using Owin;


namespace Api
{
    /// <summary>
    /// Входня точка Owin
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Настройки bearer middleware
        /// </summary>
        public static OAuthBearerAuthenticationOptions OAuthBearerAuthenticationOptions { get; private set; }

        /// <summary>
        /// Настройки
        /// </summary>
        public void ConfigureAuth(IAppBuilder app)
        {
            OAuthBearerAuthenticationOptions = new OAuthBearerAuthenticationOptions();

            IUnityContainer container = UnityConfig.GetConfiguredContainer();
            container.RegisterInstance(OAuthBearerAuthenticationOptions);
            
            app.UseOAuthBearerAuthentication(OAuthBearerAuthenticationOptions);
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Ingenius Systems">
//   Copyright (c) Ingenius Systems
//   Create on 28.10.2015 11:21:57 by Albert Zakiev
// </copyright>
// <summary>
//   Defines the Startup type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Web.Mvc;
using Api;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
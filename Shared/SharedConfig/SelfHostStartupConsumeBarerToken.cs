﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.SelfHost;
using System.Web.Http.SelfHost.Channels;
using System.Web.Mvc;
using HelpPageForSelfHost.Areas.HelpPage;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Owin;

namespace SharedConfig
{
    public class SelfHostStartupConsumeBarerToken
    {
        public static HttpSelfHostConfiguration config;
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public virtual void Configuration(IAppBuilder appBuilder)
        {
            //if (ConfigurationManager.AppSettings["EnableClientIpRestriction"] != null
            //    && ConfigurationManager.AppSettings["EnableClientIpRestriction"].ToLower() == "true")
            //appBuilder.Use<ClientIpRestrictionMiddleware>();
            // Configure Web API for self-host. 
#if DEBUG

            config = new HttpSelfHostConfiguration(ConfigurationManager.AppSettings["ListeningEndPoint"].Replace("+", "0.0.0.0"));
#else
            config = new HttpsSelfHostConfiguration(ConfigurationManager.AppSettings["ListeningEndPoint"].Replace("+", "0.0.0.0"));
#endif
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            ConfigureAuth(appBuilder);

            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            // loading the api description xml file
            config.Services.Replace(typeof(IDocumentationProvider), new XmlDocumentationProvider(AppDomain.CurrentDomain.BaseDirectory + "//XmlDocument.xml"));
            appBuilder.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            appBuilder.UseWebApi(config);

            #region dis-allow to show null value property to client side json string.

            var jsonformatter = new JsonMediaTypeFormatter
            {
                SerializerSettings =
                {
                    NullValueHandling = NullValueHandling.Ignore
                },
            };

            config.Formatters.RemoveAt(0);
            config.Formatters.Insert(0, jsonformatter);
            config.Formatters.JsonFormatter
            .SerializerSettings
            .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            #endregion

        }

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            //app.UseCookieAuthentication(new CookieAuthenticationOptions());
            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            app.CreatePerOwinContext<ServiceUserRoleManager>(ServiceUserRoleManager.Create);
            app.CreatePerOwinContext<ServiceUserManager>(ServiceUserManager.Create);
            // Configure the application validate the incomign request with OAuth=========================
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
            {
                AccessTokenFormat = new XServiceAccessTokenFormat(new XServiceDataProtector()),
            });
            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            //app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }
    }
}

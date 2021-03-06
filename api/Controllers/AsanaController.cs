﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetAuth;
using DotNetAuth.OAuth2;

namespace web.Controllers
{
    public partial class AsanaController : Controller
    {
        ApplicationCredentials AsanaAppCredentials;
        OAuth2SessionStateManager oauth2StateManager;
#if DEBUG
        const string AppUrl = "http://localhost:8080/app/app.html";
#else
        const string AppUrl = "http://agility.azurewebsites.net/app/app.html";
#endif
   
        public AsanaController()
        {
            SetKeys();
        }

        // declared as 'partial' so we don't get an error if the Keys.cs file is missing
        partial void SetKeys();
        
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        protected override System.IAsyncResult BeginExecute(System.Web.Routing.RequestContext requestContext, System.AsyncCallback callback, object state)
        {
            oauth2StateManager = new OAuth2SessionStateManager(requestContext.HttpContext.Session);
            return base.BeginExecute(requestContext, callback, state);
        }

        public ActionResult Login()
        {
            var userProcessUri = Url.Action("AsanaResponseProcess", "Asana", routeValues: null, protocol: Request.Url.Scheme);
            var redirectUri = DotNetAuth.OAuth2.OAuth2Process.GetAuthenticationUri(new AsanaProvider(), AsanaAppCredentials, userProcessUri, null, oauth2StateManager);
            return Redirect(redirectUri.ToString());
        }
        public ActionResult AsanaResponseProcess()
        {
            var userProcessUri = Url.Action("AsanaResponseProcess", "Asana", routeValues: null, protocol: Request.Url.Scheme);
            var response = DotNetAuth.OAuth2.OAuth2Process.ProcessUserResponse(new AsanaProvider(), AsanaAppCredentials, Request.Url, userProcessUri, oauth2StateManager);
            response.Wait();
            Session["asanaaccesstoken"] = response.Result.AccessToken;
            Session["asanaexpire"] = response.Result.Expires;
            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("asana_token", response.Result.AccessToken));
            this.ControllerContext.HttpContext.Response.Cookies.Add(new HttpCookie("asana_expire", response.Result.Expires.ToString()));

            return Redirect(AppUrl);
        }


	}
}


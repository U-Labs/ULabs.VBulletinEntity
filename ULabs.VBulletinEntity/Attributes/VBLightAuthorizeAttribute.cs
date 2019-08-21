using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;
using ULabs.VBulletinEntity.LightManager;
using ULabs.VBulletinEntity.Manager;

namespace ULabs.VBulletinEntity.Attributes {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class VBLightAuthorizeAttribute : Attribute, IResourceFilter {
        string loginRedirectUrl;
        public VBLightAuthorizeAttribute(string loginRedirectUrl = "") {
            this.loginRedirectUrl = loginRedirectUrl;
        }

        T GetService<T>(ResourceExecutingContext context) {
            var service = context.HttpContext.RequestServices.GetService(typeof(T));
            return (T)service;
        }

        public void OnResourceExecuting(ResourceExecutingContext context) {
            var lightSessionManager = GetService<VBLightSessionManager>(context);
            var userSession = lightSessionManager.GetCurrent();
            // Session shouldn't be null any more since we implemented guest sessions, too
            if (!userSession.IsLoggedIn) {
                if (string.IsNullOrEmpty(loginRedirectUrl)) {
                    var settingsManager = GetService<VBSettingsManager>(context);
                    loginRedirectUrl = settingsManager.GetCommonSettings().BaseUrl;
                }

                context.HttpContext.Response.StatusCode = 401;
                context.Result = new RedirectResult(loginRedirectUrl);
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context) {

        }
    }
}
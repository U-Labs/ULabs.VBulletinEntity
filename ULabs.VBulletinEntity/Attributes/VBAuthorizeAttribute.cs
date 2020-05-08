using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;
using ULabs.VBulletinEntity.Manager;

namespace ULabs.LightVBulletinEntity.Attributes {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class VBAuthorizeAttribute : Attribute, IResourceFilter {
        string loginRedirectUrl;
        public VBAuthorizeAttribute(string loginRedirectUrl = "") {
            this.loginRedirectUrl = loginRedirectUrl;
        }

        T GetService<T>(ResourceExecutingContext context) {
            var service = context.HttpContext.RequestServices.GetService(typeof(T));
            return (T)service;
        }

        public void OnResourceExecuting(ResourceExecutingContext context) {
            var sessionManager = GetService<VBSessionManager>(context);
            var userSession = sessionManager.GetCurrentAsync().Result;
            // Session shouldn't be null any more since we implemented guest sessions, too
            if (!userSession.LoggedIn) {
                if (string.IsNullOrEmpty(loginRedirectUrl)) {
                    var settingsManager = GetService<VBSettingsManager>(context);
                    loginRedirectUrl = settingsManager.GetCommonSettings().BaseUrl;
                }
                
                context.Result = new RedirectResult(loginRedirectUrl);
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context) {

        }
    }
}
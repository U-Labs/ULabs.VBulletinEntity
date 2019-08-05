using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;
using ULabs.VBulletinEntity.Manager;

namespace ULabs.VBulletinEntity.Attributes {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class VBAuthorizeAttribute : Attribute, IResourceFilter {
        public VBAuthorizeAttribute() {

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
                var settingsManager = GetService<VBSettingsManager>(context);
                // ToDo: Redirect to specific login page if exists (login.php redirects to index)
                context.Result = new RedirectResult(settingsManager.GetCommonSettings().BaseUrl);
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context) {

        }
    }
}
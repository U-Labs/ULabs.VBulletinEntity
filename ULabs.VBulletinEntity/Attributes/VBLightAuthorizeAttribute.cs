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
        // Using 0 as default value because nullable ints are not allowed here: https://stackoverflow.com/questions/3765339/nullable-int-in-attribute-constructor-or-property
        int requiredUserGroupId;
        string permissionRedirectUrl;

        /// <summary>
        /// Redirects the user to the given url if he's not authenticated or not a member of the required group
        /// </summary>
        /// <param name="loginRedirectUrl">The url where the user gets redirected to if he's not authenticated</param>
        /// <param name="permissionRedirectUrl">The redirect url if the user is authenticated, but not a member of <paramref name="requiredUserGroupId"/> (if set)</param>
        /// <param name="requiredUserGroupId">Requires that the user is member of the specified group id. Otherwise he's redirected to <paramref name="permissionRedirectUrl"/>.</param>
        public VBLightAuthorizeAttribute(string loginRedirectUrl = "", string permissionRedirectUrl = "", int requiredUserGroupId = 0) {
            this.loginRedirectUrl = loginRedirectUrl;
            this.permissionRedirectUrl = permissionRedirectUrl;
            this.requiredUserGroupId = requiredUserGroupId;

            if(!string.IsNullOrEmpty(permissionRedirectUrl) && requiredUserGroupId == 0) {
                throw new Exception("When requiredUserGroupId is set, you need to also specify the redirection url in case of permission mismatches.");
            }
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

            bool requiredGroupMissing = requiredUserGroupId > 0 && userSession.User.PrimaryUserGroup.Id != requiredUserGroupId;
            if (requiredGroupMissing) {
                context.Result = new RedirectResult(permissionRedirectUrl);
            }
        }

        public void OnResourceExecuted(ResourceExecutedContext context) {

        }
    }
}
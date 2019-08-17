using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.Tools {
    /// <summary>
    /// Contains some helper functions for session handling that can be used in VBSessionManager as well as in VBLightSessionManager
    /// </summary>
    public class VBSessionHelper {
        readonly IHttpContextAccessor contextAccessor;
        public VBSessionHelper(IHttpContextAccessor contextAccessor) {
            this.contextAccessor = contextAccessor;
        }
        public string GenerateHash() {
            // 32 Characters long and more random than the original fetch_sessionhash() method from vB in includes/class_core.php
            return Guid.NewGuid().ToString("N");
        }
        public string GenerateIdHash() {
            // define('SESSION_IDHASH', md5($_SERVER['HTTP_USER_AGENT'] . $this->fetch_substr_ip($this->getIp())));
            string ipBeginning = GetClientIpAddress();
            ipBeginning = ipBeginning.Substring(0, ipBeginning.LastIndexOf('.'));

            string idHash = Hash.Md5($"{GetUserAgent()}{ipBeginning}");
            return idHash;
        }
        public string GetClientIpAddress() {
            // ToDo: Test if https://stackoverflow.com/a/41335701/3276634 works remote and secure it for prod env (accept those headers only from trusted ips/networks)
            return contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
        
        public string GetUserAgent() {
            var requestHeaders = contextAccessor.HttpContext.Request.Headers;
            if (requestHeaders.ContainsKey(HeaderNames.UserAgent)) {
                return requestHeaders[HeaderNames.UserAgent];
            }
            return "";
        }
    }
}

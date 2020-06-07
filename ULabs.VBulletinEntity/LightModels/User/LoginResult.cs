using System;
using System.Collections.Generic;
using System.Text;

namespace ULabs.VBulletinEntity.LightModels.User {
    public class CheckPasswordResult {
        public LoginResult LoginResult { get; set; }
        public string CookiePassword { get; set; }
        public int UserId { get; set; }
        public CheckPasswordResult(LoginResult loginResult, string cookiePassword = "") {
            LoginResult = loginResult;
            CookiePassword = cookiePassword;
        }
    }
    public enum LoginResult {
        Success,
        UserNotExisting,
        BadPassword,
        StrikesLimitReached
    }
}

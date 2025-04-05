using System;
using System.Collections.Generic;
using System.Text;

namespace Rhinero.Utils.Common.Http.User
{
    public static class ValidUserInfo
    {
        public static string Token { get; set; }
        public static int? ValidUserId { get; set; }
        public static string ValidUsername { get; set; }
        public static string FullName { get; set; }
        public static string Cookie { get; set; }


        public static LoggedInUserResponseModel loggedInUser { get; set; }


        public static void SetValidUser(LoggedInUserResponseModel user)
        {
            Token = user.AuthToken.ToString();
            ValidUserId = user.Id;
            ValidUsername = user.Email;
            FullName = user.FirstName + " " + user.LastName;
            loggedInUser = user;
        }

        public static void Logout()
        {
            loggedInUser = null;
            Token = null;
            ValidUserId = null;
            ValidUsername = null;
        }
    }
}

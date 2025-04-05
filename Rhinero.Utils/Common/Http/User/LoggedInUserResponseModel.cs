using System;
using System.Collections.Generic;
using System.Text;

namespace Rhinero.Utils.Common.Http.User
{
    public class LoggedInUserResponseModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsValid { get; set; }

        public Guid? AuthToken { get; set; }
        public DateTime? AuthTokenExpireTime { get; set; }
    }
}

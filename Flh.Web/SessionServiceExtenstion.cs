using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Flh.Web
{

    public static class SessionServiceExtenstion
    {
        public static UserSessionEntry GetCurrentUser(this HttpSessionStateBase session)
        {
            return session[Config.Current.SESSION_USER_KEY] as UserSessionEntry;
        }

        public static void SetCurrentUser(this HttpSessionStateBase session, UserSessionEntry entry)
        {
            session[Config.Current.SESSION_USER_KEY] = entry;
        }
        public static  string GetCurrentCertCode(this HttpSessionStateBase session)
        {
             return HttpContext.Current.Session[Config.Current.CERT_CODE] as string;
        }
        public static void SetCurrentCertCode(this HttpSessionStateBase session, string code)
        {
            HttpContext.Current.Session[Config.Current.CERT_CODE] = code;
        }
        public static UserSessionEntry Login(this HttpSessionStateBase session, Flh.Business.IUserManager manager, string username, string password, string ip)
        {
            var user = manager.Login(username, password, ip);
            var entry = new UserSessionEntry
            {
                Name = user.Name,
                Uid = user.Uid
            };
            SetCurrentUser(session, entry);
            return entry;
        }
    }
}

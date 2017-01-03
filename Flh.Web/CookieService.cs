using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Flh.Web
{
    public interface ICookieService
    {
        /// <summary>
        /// Response中的Cookie
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        HttpCookie this[string name] { get; }
        /// <summary>
        /// Response, Request中的Cookie
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        HttpCookie Get(string name);
        /// <summary>
        /// Response中的Cookie
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        void Set(HttpCookie cookie);
        void Del(string name);
        HttpCookie GetRequestCookie(string name);

        CookieUser User { get; set; }
        void Logout();
    }

    public class CookieServiceImpl : ICookieService
    {
        private readonly HttpContext m_Context;
        private static Flh.Security.DES m_DES;

        public CookieServiceImpl(HttpContext context)
        {
            ExceptionHelper.ThrowIfNull(context, "context");
            m_Context = context;
            m_DES = new Flh.Security.DES("fuliaohui");
        }

        public CookieServiceImpl() : this(HttpContext.Current) { }

        public HttpCookie this[string name]
        {
            get
            {
                if (m_Context.Response.Cookies.AllKeys.Contains(name))
                {
                    return m_Context.Response.Cookies.Get(name);
                }
                else
                {
                    return null;
                }
            }
        }

        public HttpCookie GetRequestCookie(string name)
        {
            var cookie = m_Context.Request.Cookies.Get(name);
            return cookie == null || String.IsNullOrEmpty(cookie.Value) ? null : cookie;
        }


        public HttpCookie Get(string name)
        {
            return GetRequestCookie(name) ?? this[name];
        }
        public CookieUser User
        {
            get
            {
                var cookie = Get(Config.Current.COOKIE_REMEMBER_USER_KEY);
                if (cookie == null) return null;
                var value = String.Empty;
                try
                {
                    value = m_DES.DesDecryptFixKey(cookie.Value);
                    var r = JsonConvert.DeserializeObject<CookieUser>(value);
                    r.LogoutAction = Logout;
                    return r;
                }
                catch (System.Exception ex)
                {
                    return null;
                }
            }
            set
            {
                var v = value == null ? String.Empty : m_DES.DesEncryptFixKey(JsonConvert.SerializeObject(value));
                var cookie = new HttpCookie(Config.Current.COOKIE_REMEMBER_USER_KEY, v);
                cookie.Expires = DateTime.Now.AddYears(1);
                Set(cookie);
            }
        }

        public void Logout()
        {
            var user = User;
            if (user != null)
            {
                Del(Config.Current.COOKIE_REMEMBER_USER_KEY);
                User = null;
            }
        }

        public void Del(string name)
        {
            var cookie = this[name];
            if (cookie == null)
            {
                cookie = new HttpCookie(name, String.Empty);
            }
            cookie.Expires = DateTime.Now.AddDays(-1);
            Set(cookie);
        }


        public void Set(HttpCookie cookie)
        {
            if (this[cookie.Name] == null)
                m_Context.Response.Cookies.Add(cookie);
            else
                m_Context.Response.Cookies.Set(cookie);
        }
    }

    public class CookieUser
    {
        public long id { get; set; }
        public string un { get; set; }
        public string pwd { get; set; }
        public CookieUserState state { get; set; }
        internal Action LogoutAction { get; set; }
        public CookieUser() { }

        public CookieUser(long id, string username, string password, CookieUserState state)
        {
            this.id = id;
            this.pwd = password;
            this.state = state;
            this.un = username;
        }

        public CookieUser(long id, string username, string password, bool? remember)
            : this(id, username, password, remember.HasValue && remember.Value ? CookieUserState.Remember : CookieUserState.Logged)
        {

        }
    }
    public enum CookieUserState
    {
        Logout = 0,
        Logged = 1,
        Remember = 2,
    }
}

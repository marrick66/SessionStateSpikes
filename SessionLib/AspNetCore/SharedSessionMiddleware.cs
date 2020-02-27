using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLib.AspNetCore
{
    /// <summary>
    /// If a request contains a "sessionid" query, this middleware attempts
    /// to bootstrap the shared session into the local request.
    /// </summary>
    public class SharedSessionMiddleware
    {
        private const string SESSION_ID_QUERY_KEY = "sessionid";
        
        private readonly RequestDelegate _next;
        private readonly ISessionStore _sessionStore;
        private readonly IDataProtector _protector;
        private readonly SessionOptions _options;

        public SharedSessionMiddleware(
            RequestDelegate next,
            ISessionStore sessionStore,
            IDataProtectionProvider protectionProvider,
            IOptions<SessionOptions> options)
        {
            _next = next
                ?? throw new ArgumentNullException(nameof(next));
            _sessionStore = sessionStore
                ?? throw new ArgumentNullException(nameof(sessionStore));
            _options = options == null
                ? throw new ArgumentNullException(nameof(options))
                : options.Value;

            //The session middleware uses an IDataProtector with
            //a hardcoded "SessionMiddleware" purpose. For the session
            //cookie value to be successfully encrypted, we have to do the same.
            //The more permanent way to do this is use a custom IDataProtectionProvider that
            //returns a shared IDataProtector instead (via DI).
            _protector = protectionProvider == null
                ? throw new ArgumentNullException(nameof(protectionProvider))
                : protectionProvider.CreateProtector("SessionMiddleware");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Query.ContainsKey(SESSION_ID_QUERY_KEY))
            {
                var key = context.Request.Query[SESSION_ID_QUERY_KEY];
                var session = GetExistingSession(key);

                //Remove querystring to get a clean page(as opposed)
                //to making the controller redirect to it:
                context.Request.QueryString = QueryString.Empty;

                if (session.Keys.Any())
                {
                    //Replace the existing session cookie,
                    //which may occur if the site's visited prior to
                    //submitted a valid shared session key.
                    SetSessionCookie(context, key);

                    //This part overwrites the session with the shared
                    //one in the HttpContext.
                    var feature = new SessionFeature
                    {
                        Session = session
                    };

                    context.Features.Set<ISessionFeature>(feature);

                    try
                    {
                        await _next(context);
                    }
                    finally
                    {
                        context.Features.Set<ISessionFeature>(null);

                        if (feature.Session != null)
                        {
                            try
                            {
                                await feature.Session.CommitAsync(context.RequestAborted);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }

                    return;
                }

            }

            await _next(context);
        }

        private ISession GetExistingSession(string key)
        {
            return _sessionStore
                .Create(
                    key,
                    TimeSpan.FromMinutes(20),
                    TimeSpan.FromSeconds(30),
                    () => true,
                    false);
        }

        /// <summary>
        /// Copied this from the decompiled SessionMiddleware, since it's
        /// not accessible publicly.
        /// </summary>
        private void SetSessionCookie(HttpContext context, string value)
        { 
            var cookie = _options.Cookie.Build(context);
            var valueStr = Protect(value);

            context.Response.Cookies.Append(_options.Cookie.Name, valueStr, cookie);
            context.Response.Headers["Cache-Control"] = "no-cache";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "-1";
        }

        /// <summary>
        /// Also copied this from the decompiled SessionMiddleware, since it's
        /// not accessible publicly.
        /// </summary>
        private string Protect(string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            var protBytes = _protector.Protect(valueBytes);

            return Convert.ToBase64String(
                    protBytes)
                .TrimEnd('=');
        }
    }
}

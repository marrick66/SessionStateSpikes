using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SessionLib.AspNetCore
{
    /// <summary>
    /// This service allows management of ASP.Net Core session values that
    /// are stored in an IDistributedCache instance (Sql, Redis, etc). 
    /// </summary>
    public class DistributedCacheSessionService
        : ISessionService
    {
        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();
        
        private ISessionStore _sessionStore;
        private IDistributedCache _cache;
        private SessionOptions _options;

        public DistributedCacheSessionService(
            ISessionStore sessionStore,
            IDistributedCache cache,
            IOptions<SessionOptions> options)
        {
            _sessionStore = sessionStore
                    ?? throw new ArgumentNullException(nameof(sessionStore));
            _cache = cache
                    ?? throw new ArgumentNullException(nameof(cache));
            _options = options == null
                ? throw new ArgumentNullException(nameof(options))
                : options.Value;
        }

        /// <summary>
        /// If the session exists, all values are cleared and it's
        /// removed from the cache.
        /// </summary>
        public Task Delete(string key)
        {
            var session = CreateOrGetSession(key);
            session?.Clear();

            return _cache.RemoveAsync(key);
        }

        /// <summary>
        /// Stores the key/json value pair in the same binary format used by
        /// ASP.Net Core's session state, so it can be integrated in other web applications.
        /// </summary>
        public async Task<string> Create(ICollection<SessionKeyJsonValue> sessionValues)
        {
            if(sessionValues == null)
                throw new ArgumentNullException(nameof(sessionValues));

            var newKey = GenerateSessionKey();
            var session = CreateOrGetSession(newKey, true);

            SetValues(session, sessionValues);

            await session.CommitAsync();

            return newKey;
        }

        /// <summary>
        /// Returns values for an existing session.
        /// </summary>
        public Task<ICollection<SessionKeyJsonValue>> Get(string key)
        {
            var session = CreateOrGetSession(key);

            if (session == null || !session.Keys.Any())
                return Task.FromResult(null as ICollection<SessionKeyJsonValue>);

            var sessionValues = new List<SessionKeyJsonValue>();

            foreach (var sessionKey in session.Keys)
            {
                if (session.TryGetValue(sessionKey, out var value))
                {
                    sessionValues.Add(
                        new SessionKeyJsonValue
                        {
                            Key = sessionKey,
                            JsonValue = JObject.Parse(
                                Encoding.UTF8.GetString(value))
                        });
                }
            }

            return Task.FromResult<ICollection<SessionKeyJsonValue>>(sessionValues);
        }

        /// <summary>
        /// Currently just sets the values submitted. Doesn't affect existing
        /// values not in the request. Could have a separate call to replace the existing values
        /// with those submitted, if a use case is needed.
        /// </summary>
        public async Task Save(string key, ICollection<SessionKeyJsonValue> sessionValues)
        {
            var session = CreateOrGetSession(key);

            if (session != null && session.Keys.Any())
            {
                SetValues(session, sessionValues);
                await session.CommitAsync();
            }
        }

        private void SetValues(ISession session, ICollection<SessionKeyJsonValue> sessionValues)
        {
            foreach (var pair in sessionValues)
            {
                session.Set(
                    pair.Key,
                    Encoding.UTF8.GetBytes(pair.JsonValue.ToString()));
            }
        }

        private ISession CreateOrGetSession(string key, bool isNew = false)
        {
            return _sessionStore.Create(
                    key,
                    _options.IdleTimeout,
                    _options.IOTimeout,
                    () => true,
                    isNew);
        }

        private string GenerateSessionKey()
        {
            var array = new byte[16];
            CryptoRandom.GetBytes(array);
            return new Guid(array).ToString();
        }
    }
}

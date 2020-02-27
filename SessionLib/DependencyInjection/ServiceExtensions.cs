using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using SessionLib.AspNetCore;

namespace SessionLib.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSqlSessionService(
            this IServiceCollection services,
            string keyPath,
            Action<SqlServerCacheOptions> configureCache,
            Action<SessionOptions> configureSession)
        {
            services.Add(ServiceDescriptor.Transient<ISessionService, DistributedCacheSessionService>());

            //For cookie encryption/decryption in a web farm, there
            //needs to be a shared key accessible to all.
            services
                .AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keyPath));

            return services
                .Configure(configureCache)
                .AddDistributedSqlServerCache(configureCache)
                .AddSession();
        }

        public static IServiceCollection AddSharedSessions(
            this IServiceCollection services,
            string keyPath,
            Action<SessionOptions> configureSession,
            Action<SqlServerCacheOptions> configureCache)
        {
            return services
                .AddSqlSessionService(
                    keyPath,
                    configureCache,
                    configureSession);
        }
    }
}

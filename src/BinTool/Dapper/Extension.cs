using BinTool.Dapper.Sqlite;
using BinTool.Dapper.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace BinTool.Dapper
{
    public static class BinToolDapperExtension
    {
        internal static ServiceProvider? Provider;
        
        public static IServiceCollection UseDapperMysql(
            this IServiceCollection services, Action<DapperOptions> options)
        {
            services.AddOptions().Configure<DapperOptions>(options);
            ServiceCollectionServiceExtensions.AddScoped<DapperContext>(services);
            return services;
        }

        public static IServiceCollection AddSqlServerDapper<TDapperContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction,ServiceLifetime lifetime=ServiceLifetime.Scoped)
            where TDapperContext : SqlServerDapperContext
        {
            if (optionsAction == null)
            {
                throw new ArgumentNullException(nameof(optionsAction));
            }

            var builder = new DbContextOptionsBuilder();
            optionsAction.Invoke(builder);

            services.Add(new ServiceDescriptor(typeof(TDapperContext), typeof(TDapperContext), lifetime));

            Provider ??= services.BuildServiceProvider();
            return services;
        }

        public static IServiceCollection AddSqliteDapper<TDapperContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction,ServiceLifetime lifetime=ServiceLifetime.Scoped)
            where TDapperContext : SqliteDapperContext
        {
            if (optionsAction == null)
            {
                throw new ArgumentNullException(nameof(optionsAction));
            }

            var builder = new DbContextOptionsBuilder();
            optionsAction.Invoke(builder);

            services.Add(new ServiceDescriptor(typeof(TDapperContext), typeof(TDapperContext), lifetime));

            Provider ??= services.BuildServiceProvider();
            return services;
        }

    }
}

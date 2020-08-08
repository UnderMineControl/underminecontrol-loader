using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;

namespace UnderMineControl.Loader.Setup
{
    public interface ISetupUtility
    {
        ISetupUtility AddTransient<T1, T2>() where T2 : class, T1 where T1 : class;
        ISetupUtility AddSingleton<T1>(T1 item) where T1 : class;
        T Build<T>();
        ISetupUtility AddLogging();
        ISetupUtility Configure(Action<IServiceCollection> action);
    }

    public class SetupUtility : ISetupUtility
    {
        private readonly IServiceCollection _collection;
        private readonly IConfiguration _config;

        public SetupUtility(IServiceCollection collection, IConfiguration config)
        {
            _collection = collection;
            _config = config;
            _collection.AddSingleton(config);
        }

        public ISetupUtility AddTransient<T1, T2>() where T2 : class, T1
                                           where T1 : class
        {
            _collection.AddTransient(typeof(T1), typeof(T2));
            return this;
        }

        public ISetupUtility AddSingleton<T1>(T1 item) where T1 : class
        {
            _collection.AddSingleton(typeof(T1), item);
            return this;
        }

        public T Build<T>()
        {
            return _collection.BuildServiceProvider()
                              .GetRequiredService<T>();
        }

        public ISetupUtility AddLogging()
        {
            _collection.AddLogging(_ =>
            {
                _.ClearProviders();
                _.SetMinimumLevel(LogLevel.Trace);
                _.AddNLog(_config);
            });
            return this;
        }

        public ISetupUtility Configure(Action<IServiceCollection> action)
        {
            action?.Invoke(_collection);
            return this;
        }

        public static ISetupUtility Start(IConfiguration config)
        {
            return new SetupUtility(new ServiceCollection(), config);
        }
    }
}

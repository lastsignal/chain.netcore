using System;
using Microsoft.Extensions.DependencyInjection;

namespace Chain
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureChain<T>(this IServiceCollection services, Action<ChainConfiguration<T>> options) where T : class
        {
            options(new ChainConfiguration<T>(services));
        }
    }
}

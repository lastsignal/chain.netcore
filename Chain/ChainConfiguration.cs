using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Chain
{
    public class ChainConfiguration<TInterface>
    {
        private readonly IServiceCollection _services;
        private readonly Dictionary<Type, ServiceLifetime> _linkImplementations= new Dictionary<Type, ServiceLifetime>();
        private readonly Type _linkInterface = typeof(TInterface);

        public ChainConfiguration(IServiceCollection services)
        {
            _services = services;
        }

        public void AddScoped<TImplementation>() where TImplementation : TInterface
        {
            _linkImplementations.Add(typeof(TImplementation), ServiceLifetime.Scoped);
        }

        public void AddTransient<TImplementation>() where TImplementation : TInterface
        {
            _linkImplementations.Add(typeof(TImplementation), ServiceLifetime.Transient);
        }

        public void AddSingleton<TImplementation>() where TImplementation : TInterface
        {
            _linkImplementations.Add(typeof(TImplementation), ServiceLifetime.Singleton);
        }

        public void Configure()
        {
            if (!_linkImplementations.Any())
                throw new InvalidOperationException($"At least one implementation is required before configuring '{_linkInterface.Name}'");

            foreach (var key in _linkImplementations.Keys)
            {
                RegisterImplementation(key, _linkImplementations[key]);
            }
        }

        private void RegisterImplementation(Type currentLinkType, ServiceLifetime registrationType)
        {
            var nextLink = _linkImplementations.Keys
                .SkipWhile(type => type != currentLinkType)
                .SkipWhile(type => type == currentLinkType)
                .FirstOrDefault();

            var parameterExpression = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");

            var constructor = currentLinkType.GetConstructors().Single();

            var parameters = constructor.GetParameters().Select(p =>
            {
                // next link constructor parameter
                if (_linkInterface.IsAssignableFrom(p.ParameterType))
                {
                    return nextLink is null
                        ? (Expression)Expression.Constant(null, _linkInterface)
                        : Expression.Call(typeof(ServiceProviderServiceExtensions), "GetRequiredService", new[] { nextLink }, parameterExpression);
                }

                // other constructor parameter
                return (Expression)Expression.Call(typeof(ServiceProviderServiceExtensions), "GetRequiredService", new[] { p.ParameterType }, parameterExpression);
            });

            var newExpression = Expression.New(constructor, parameters.ToArray());
            var implementation = currentLinkType == _linkImplementations.Keys.First() ? _linkInterface : currentLinkType;
            var expressionType = Expression.GetFuncType(typeof(IServiceProvider), implementation);
            var lambdaExpression = Expression.Lambda(expressionType, newExpression, parameterExpression);
            var compiledExpression = (Func<IServiceProvider, object>)lambdaExpression.Compile();

            switch (registrationType)
            {
                case ServiceLifetime.Transient:
                    _services.AddTransient(implementation, compiledExpression);
                    break;
                case ServiceLifetime.Scoped:
                    _services.AddScoped(implementation, compiledExpression);
                    break;
                case ServiceLifetime.Singleton:
                    _services.AddSingleton(implementation, compiledExpression);
                    break;
            }
        }
    }
}

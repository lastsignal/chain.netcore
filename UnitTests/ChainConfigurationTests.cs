using System;
using Chain;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace UnitTests;

public class ChainConfigurationTests
{
    private interface ILink{}

    private class InitialLink(ILink nextLink) : ILink;
    private class IntermediateLink1(ILink nextLink): ILink;
    private class IntermediateLink2(ILink nextLink): ILink;
    private class LastLink(ILink nextLink): ILink;

    [Fact]
    public void Configure_without_registration_should_be_invalid()
    {
        IServiceCollection sc = new ServiceCollection();

        Should.Throw<InvalidOperationException>(() =>
            sc.ConfigureChain<ILink>(options =>
            {
                options.Configure();
            })
        );
    }

    [Fact]
    public void Configure_should_register_all_provided_types()
    {
        var collection = new ServiceCollection();

        collection.ConfigureChain<ILink>(options =>
        {
            options.AddSingleton<InitialLink>();
            options.AddSingleton<IntermediateLink1>();
            options.AddSingleton<IntermediateLink2>();
            options.AddSingleton<LastLink>();
            options.Configure();
        });

        collection.Count.ShouldBe(4);

        foreach (var descriptor in collection)
        {
            typeof(ILink).IsAssignableFrom(descriptor.ServiceType).ShouldBeTrue();
            descriptor.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        }

        // The first implementation is registered under the chain interface.
        collection.ShouldContain(x =>
            x.Lifetime == ServiceLifetime.Singleton &&
            x.ServiceType == typeof(ILink)
        );

        // Subsequent implementations are registered under their own type.
        collection.ShouldContain(x =>
            x.Lifetime == ServiceLifetime.Singleton &&
            x.ServiceType == typeof(IntermediateLink1)
        );

        collection.ShouldContain(x =>
            x.Lifetime == ServiceLifetime.Singleton &&
            x.ServiceType == typeof(IntermediateLink2)
        );

        collection.ShouldContain(x =>
            x.Lifetime == ServiceLifetime.Singleton &&
            x.ServiceType == typeof(LastLink)
        );
    }

    [Fact]
    public void Service_registry_should_return_the_initial_link_for_the_service_type()
    {
        var collection = new ServiceCollection();

        collection.ConfigureChain<ILink>(options =>
        {
            options.AddSingleton<InitialLink>();
            options.AddSingleton<IntermediateLink1>();
            options.AddSingleton<IntermediateLink2>();
            options.AddSingleton<LastLink>();
            options.Configure();
        });

        var provider = collection.BuildServiceProvider();

        var service = provider.GetRequiredService<ILink>();

        service.ShouldBeOfType<InitialLink>();
    }
}

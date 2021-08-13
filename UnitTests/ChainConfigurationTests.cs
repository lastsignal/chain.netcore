using System;
using Chain;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace UnitTests
{
    public interface ILink{}

    public class Link1: ILink
    {
        private ILink _nextLink;

        public Link1(ILink nextLink)
        {
            _nextLink = nextLink;
        }
    }

    public class Link2: ILink
    {
        public Link2()
        {
            
        }
    }

    public class ChainConfigurationTests
    {
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

            // collection.AddSingleton<ILink, Link2>();

            collection.ConfigureChain<ILink>(options =>
            {
                options.AddSingleton<Link1>();
                options.AddSingleton<Link2>();
                options.Configure();
            });

            var descriptor = Assert.Single(collection);

            var serviceType = collection[0].ServiceType;

            // descriptor.ServiceType.ShouldBe(typeof(ILink));
            // descriptor.ImplementationType.ShouldBe(typeof(Link2));

            // typeof(ILink).IsAssignableFrom(serviceType).ShouldBeTrue();
            // serviceType.IsAssignableFrom(typeof(Link1)).ShouldBeTrue();
            // serviceType.IsAssignableFrom(typeof(Link2)).ShouldBeTrue();

            // sc.ShouldContain(x =>
            //     x.Lifetime == ServiceLifetime.Singleton &&
            //     x.ServiceType == typeof(Link1)
            // );


        }
    }
}

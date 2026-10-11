using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace IPTech.Utils.Tests {
    public class ServiceContainerTests {
        private ServiceContainer serviceContainer;

        [SetUp]
        public void SetUp() {
            serviceContainer = new ServiceContainer();
        }

        [Test]
        public void AddService_instance_can_be_queried_by_generic_and_type() {
            var service = new TestService();
            serviceContainer.AddService(service);

            Assert.That(serviceContainer.HasService<TestService>(), Is.True);
            Assert.That(serviceContainer.HasService(typeof(TestService)), Is.True);
            Assert.That(serviceContainer.HasServiceInstance<TestService>(), Is.True);
            Assert.That(serviceContainer.HasServiceInstance(typeof(TestService)), Is.True);
            Assert.That(serviceContainer.GetService<TestService>(), Is.SameAs(service));
            Assert.That(serviceContainer.GetService(typeof(TestService)), Is.SameAs(service));
        }

        [Test]
        public void TryGetService_returns_false_and_null_for_an_unregistered_service() {
            Assert.That(serviceContainer.TryGetService<TestService>(out var genericService), Is.False);
            Assert.That(genericService, Is.Null);
            Assert.That(serviceContainer.TryGetService(typeof(TestService), out var service), Is.False);
            Assert.That(service, Is.Null);
        }

        [Test]
        public void GetService_throws_for_an_unregistered_service() {
            Assert.Throws<KeyNotFoundException>(() => serviceContainer.GetService<TestService>());
        }

        [Test]
        public void AddService_factory_creates_and_caches_the_service_on_first_resolution() {
            var creationCount = 0;
            IServiceLocator factoryLocator = null;
            serviceContainer.AddService<TestService>((locator, type) => {
                creationCount++;
                factoryLocator = locator;
                Assert.That(type, Is.EqualTo(typeof(TestService)));
                return new TestService();
            });

            Assert.That(serviceContainer.HasService<TestService>(), Is.True);
            Assert.That(serviceContainer.HasServiceInstance<TestService>(), Is.False);
            Assert.That(creationCount, Is.Zero);

            var first = serviceContainer.GetService<TestService>();
            var second = serviceContainer.GetService<TestService>();

            Assert.That(first, Is.SameAs(second));
            Assert.That(factoryLocator, Is.SameAs(serviceContainer));
            Assert.That(serviceContainer.HasServiceInstance<TestService>(), Is.True);
            Assert.That(creationCount, Is.EqualTo(1));
        }

        [Test]
        public void Factory_can_resolve_a_registered_dependency() {
            var dependency = new TestDependency();
            serviceContainer.AddService(dependency);
            serviceContainer.AddService<TestService>((locator, _) =>
                new TestService { Dependency = locator.GetService<TestDependency>() });

            var service = serviceContainer.GetService<TestService>();

            Assert.That(service.Dependency, Is.SameAs(dependency));
        }

        [Test]
        public void Factory_that_returns_null_throws_and_does_not_create_an_instance() {
            serviceContainer.AddService<TestService>((_, __) => null);

            Assert.Throws<Exception>(() => serviceContainer.GetService<TestService>());
            Assert.That(serviceContainer.HasServiceInstance<TestService>(), Is.False);
        }

        [Test]
        public void Service_added_and_removed_events_report_the_service_type() {
            var addedTypes = new List<Type>();
            var removedTypes = new List<Type>();
            serviceContainer.ServiceAdded += addedTypes.Add;
            serviceContainer.ServiceRemoved += removedTypes.Add;

            serviceContainer.AddService(new TestService());
            serviceContainer.RemoveService<TestService>();

            Assert.That(addedTypes, Is.EqualTo(new[] { typeof(TestService) }));
            Assert.That(removedTypes, Is.EqualTo(new[] { typeof(TestService) }));
            Assert.That(serviceContainer.HasService<TestService>(), Is.False);
        }

        [Test]
        public void Factory_cannot_resolve_a_service_while_it_is_being_created() {
            serviceContainer.AddService<TestService>((locator, _) => locator.GetService<TestService>());

            var exception = Assert.Throws<Exception>(() => serviceContainer.GetService<TestService>());

            Assert.That(exception.Message, Does.Contain("Circular reference detected"));
        }

        private sealed class TestService {
            public TestDependency Dependency { get; set; }
        }

        private sealed class TestDependency { }
    }
}
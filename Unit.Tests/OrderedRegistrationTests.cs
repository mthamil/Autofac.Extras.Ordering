using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Extras.Ordering;
using Xunit;

namespace Unit.Tests
{
    public class OrderedRegistrationTests
    {
        [Fact]
        public void Test_OrderBy_Constant_Per_Component()
        {
            // Arrange.
            _builder.RegisterType<TestComponent>()
                    .UsingOrdering();
            _builder.Register(_ => new Dependency("dep 3"))
                    .OrderBy(2);
            _builder.Register(_ => new Dependency("dep 2"))
                    .OrderBy(1);
            _builder.Register(_ => new Dependency("dep 1"))
                    .OrderBy(3);
            var container = _builder.Build();

            // Act.
            var component = container.Resolve<TestComponent>();

            // Assert.
            Assert.Equal(new[] { "dep 2", "dep 3", "dep 1" }, component.Dependencies.Select(d => d.Name));
        }

        [Fact]
        public void Test_OrderBy_Selector_Per_Component()
        {
            // Arrange.
            _builder.RegisterType<TestComponent>()
                    .UsingOrdering();
            _builder.Register(_ => new Dependency("dep 3"))
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new Dependency("dep 2"))
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new Dependency("dep 1"))
                    .OrderBy(d => d.Name);
            var container = _builder.Build();

            // Act.
            var component = container.Resolve<TestComponent>();

            // Assert.
            Assert.Equal(new[] { "dep 1", "dep 2", "dep 3" }, component.Dependencies.Select(d => d.Name));
        }

        [Fact]
        public void Test_OrderBy_Constant_Per_Module()
        {
            // Arrange.
            _builder.RegisterModule<TestModule>();
            var container = _builder.Build();

            // Act.
            var component = container.Resolve<TestComponent>();

            // Assert.
            Assert.Equal(new[] { "dep 3", "dep 1", "dep 2" }, component.Dependencies.Select(d => d.Name));
        }

        private readonly ContainerBuilder _builder = new ContainerBuilder();

        public class TestComponent
        {
            public TestComponent(IOrderedEnumerable<Dependency> dependencies)
            {
                Dependencies = dependencies;
            }

            public IEnumerable<Dependency> Dependencies { get; private set; }
        }

        public class Dependency
        {
            public Dependency(string name)
            {
                Name = name;
            }

            public string Name { get; private set; }
        }

        public class TestModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<TestComponent>()
                       .UsingOrdering();
                builder.Register(_ => new Dependency("dep 1"))
                       .OrderBy(2);
                builder.Register(_ => new Dependency("dep 3"))
                       .OrderBy(1);
                builder.Register(_ => new Dependency("dep 2"))
                       .OrderBy(3);
            }

            protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
            {
                registration.UseOrdering();
            }
        }
    }
}
using System.Linq;
using Autofac;
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
        public void Test_OrderBy_Constant_Per_Component_As_Interface()
        {
            // Arrange.
            _builder.RegisterType<TestComponent_With_InterfaceDependency>()
                    .UsingOrdering();
            _builder.Register(_ => new Dependency("dep 3")).As<IDependency>()
                    .OrderBy(3);
            _builder.Register(_ => new Dependency("dep 2")).As<IDependency>()
                    .OrderBy(1);
            _builder.Register(_ => new OtherDependency("dep 1")).As<IDependency>()
                    .OrderBy(2);
            var container = _builder.Build();

            // Act.
            var component = container.Resolve<TestComponent_With_InterfaceDependency>();

            // Assert.
            Assert.Equal(new[] { "dep 2", "dep 1", "dep 3" }, component.Dependencies.Select(d => d.Name));
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
        public void Test_OrderBy_Selector_Per_Component_As_Interface()
        {
            // Arrange.
            _builder.RegisterType<TestComponent_With_InterfaceDependency>()
                    .UsingOrdering();
            _builder.Register(_ => new Dependency("dep 3")).As<IDependency>()
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new OtherDependency("dep 2")).As<IDependency>()
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new Dependency("dep 1")).As<IDependency>()
                    .OrderBy(d => d.Name);
            var container = _builder.Build();

            // Act.
            var component = container.Resolve<TestComponent_With_InterfaceDependency>();

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

        [Fact]
        public void Test_OrderBy_Selector_Per_Module_As_Interface()
        {
            // Arrange.
            _builder.RegisterModule<TestModule_With_InterfaceDependency>();
            var container = _builder.Build();

            // Act.
            var component = container.Resolve<TestComponent_With_InterfaceDependency>();

            // Assert.
            Assert.Equal(new[] { "dep 1", "dep 2", "dep 3" }, component.Dependencies.Select(d => d.Name));
        }

        private readonly ContainerBuilder _builder = new ContainerBuilder();
    }
}
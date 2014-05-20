using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Extras.Ordering;

namespace Unit.Tests
{
    public partial class OrderedRegistrationTests
    {
        class TestComponent
        {
            public TestComponent(IOrderedEnumerable<Dependency> dependencies)
            {
                Dependencies = dependencies;
            }

            public IEnumerable<Dependency> Dependencies { get; private set; }
        }

        class TestComponent_With_InterfaceDependency
        {
            public TestComponent_With_InterfaceDependency(IOrderedEnumerable<IDependency> dependencies)
            {
                Dependencies = dependencies;
            }

            public IEnumerable<IDependency> Dependencies { get; private set; }
        }

        interface IDependency
        {
            string Name { get; }
        }

        class Dependency : IDependency
        {
            public Dependency(string name)
            {
                Name = name;
            }

            public string Name { get; private set; }
        }

        class OtherDependency : IDependency
        {
            public OtherDependency(string name)
            {
                Name = name;
            }

            public string Name { get; private set; }
        }

        class TestModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<TestComponent>();

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

        class TestModule_With_InterfaceDependency : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<TestComponent_With_InterfaceDependency>();

                builder.Register(_ => new Dependency("dep 1")).As<IDependency>()
                       .OrderBy(d => d.Name);
                builder.Register(_ => new OtherDependency("dep 3")).As<IDependency>()
                       .OrderBy(d => d.Name);
                builder.Register(_ => new Dependency("dep 2")).As<IDependency>()
                       .OrderBy(d => d.Name);
            }

            protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
            {
                registration.UseOrdering();
            }
        }
	}
}
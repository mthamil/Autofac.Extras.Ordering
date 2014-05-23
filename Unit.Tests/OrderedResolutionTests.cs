using System.Linq;
using Autofac;
using Autofac.Extras.Ordering;
using Xunit;

namespace Unit.Tests
{
    public class OrderedResolutionTests
    {
        [Fact]
        public void Test_ResolveOrdered()
        {
            // Arrange.
            _builder.Register(_ => new Dependency("dep 3")).As<IDependency>()
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new OtherDependency("dep 2")).As<IDependency>()
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new Dependency("dep 1")).As<IDependency>()
                    .OrderBy(d => d.Name);
            var container = _builder.Build();

            // Act.
            var dependencies = container.ResolveOrdered<IDependency>();

            // Assert.
            Assert.Equal(new[] { "dep 1", "dep 2", "dep 3" }, dependencies.Select(d => d.Name));
        }

        [Fact]
        public void Test_ResolveOrdered_With_Parameters()
        {
            // Arrange.
            _builder.RegisterType<Dependency>().As<IDependency>()
                    .OrderBy(2);
            _builder.RegisterType<OtherDependency>().As<IDependency>()
                    .OrderBy(1);
            _builder.RegisterType<YetAnotherDependency>().As<IDependency>()
                    .OrderBy(3);
            var container = _builder.Build();

            // Act.
            var dependencies = container.ResolveOrdered<IDependency>(TypedParameter.From("test"));

            // Assert.
            Assert.Equal(new[] { typeof(OtherDependency), typeof(Dependency), typeof(YetAnotherDependency) }, 
                         dependencies.Select(d => d.GetType()));
            Assert.Equal("test", dependencies.Select(d => d.Name).Distinct().Single());
        }

        private readonly ContainerBuilder _builder = new ContainerBuilder();
    }
}
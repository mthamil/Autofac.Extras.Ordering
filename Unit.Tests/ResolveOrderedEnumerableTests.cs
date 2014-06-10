using System.Linq;
using Autofac;
using Autofac.Extras.Ordering;
using Autofac.Features.Metadata;
using Xunit;

namespace Unit.Tests
{
    public class ResolveOrderedEnumerableTests
    {
        public ResolveOrderedEnumerableTests()
        {
            _builder.RegisterSource(new OrderedRegistrationSource());
        }

        [Fact]
        public void Test_Empty()
        {
            // Arrange.
            var container = _builder.Build();

            // Act.
            var dependencies = container.Resolve<IOrderedEnumerable<IDependency>>();

            // Assert.
            Assert.NotNull(dependencies);
            Assert.Empty(dependencies);
        }

        [Fact]
        public void Test_Resolve()
        {
            // Arrange.
            _builder.Register(_ => new Dependency("dep 2")).As<IDependency>()
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new OtherDependency("dep 3")).As<IDependency>()
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new Dependency("dep 1")).As<IDependency>()
                    .OrderBy(d => d.Name);

            var container = _builder.Build();

            // Act.
            var dependencies = container.Resolve<IOrderedEnumerable<IDependency>>();

            // Assert.
            Assert.Equal(new[] { "dep 1", "dep 2", "dep 3" }, dependencies.Select(d => d.Name));
        }

        [Fact]
        public void Test_When_Subset_Of_Types_Are_Ordered()
        {
            // Arrange.
            _builder.Register(_ => new Dependency("dep 3")).As<IDependency>()
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new OtherDependency("dep 1")).As<IDependency>();
            _builder.Register(_ => new Dependency("dep 2")).As<IDependency>()
                    .OrderBy(d => d.Name);

            var container = _builder.Build();

            // Act.
            var dependencies = container.Resolve<IOrderedEnumerable<IDependency>>();

            // Assert.
            Assert.Equal(new[] { "dep 2", "dep 3" }, dependencies.Select(d => d.Name));
        }

        [Fact]
        public void Test_With_Additional_Metadata()
        {
            // Arrange.
            _builder.Register(_ => new Dependency("dep 2")).As<IDependency>()
                    .OrderBy(x => x.Name)
                    .WithMetadata<AdditionalMetadata>(m => m.For(p => p.Data, 1));
            _builder.Register(_ => new OtherDependency("dep 3")).As<IDependency>()
                    .OrderBy(x => x.Name)
                    .WithMetadata<AdditionalMetadata>(m => m.For(p => p.Data, 2));
            _builder.Register(_ => new Dependency("dep 1")).As<IDependency>()
                    .OrderBy(x => x.Name)
                    .WithMetadata<AdditionalMetadata>(m => m.For(p => p.Data, 3));

            var container = _builder.Build();

            // Act.
            var dependencies = container.Resolve<IOrderedEnumerable<Meta<IDependency, AdditionalMetadata>>>();

            // Assert.
            Assert.Equal(new[] { "dep 1", "dep 2", "dep 3" }, dependencies.Select(d => d.Value.Name));
            Assert.Equal(new[] { 3, 1, 2 }, dependencies.Select(d => d.Metadata.Data));
        }

        [Fact]
        public void Test_Using_OrderByRegistration_For_Scanned_Types()
        {
            // Arrange.
            _builder.RegisterTypes(new[]
                    {
                        typeof(YetAnotherDependency),
                        typeof(Dependency),
                        typeof(OtherDependency)
                    })
                    .As<IDependency>()
                    .OrderByRegistration();

            var container = _builder.Build();

            // Act.
            var dependencies = container.Resolve<IOrderedEnumerable<IDependency>>();

            // Assert.
            Assert.Equal(new[] { typeof(YetAnotherDependency), typeof(Dependency), typeof(OtherDependency) },
                         dependencies.Select(d => d.GetType()));
        }

        [Fact]
        public void Test_Using_OrderByRegistration_With_Starting_Index_For_Scanned_Types()
        {
            // Arrange.
            _builder.RegisterTypes(new[]
                    {
                        typeof(OtherDependency),
                        typeof(Dependency),
                        typeof(OtherDependency)
                    })
                    .As<IDependency>()
                    .OrderByRegistration(3);

            _builder.RegisterTypes(new[]
                    {
                        typeof(YetAnotherDependency),
                        typeof(Dependency)
                    })
                    .As<IDependency>()
                    .OrderByRegistration();

            var container = _builder.Build();

            // Act.
            var dependencies = container.Resolve<IOrderedEnumerable<IDependency>>();

            // Assert.
            Assert.Equal(new[]
            {
                typeof(YetAnotherDependency), 
                typeof(Dependency), 
                typeof(OtherDependency),
                typeof(Dependency), 
                typeof(OtherDependency)
            }, dependencies.Select(d => d.GetType()));
        }
        
        [Fact]
        public void Test_When_IOrderedEnumerable_Explicitly_Registered()
        {
            // Arrange.
            _builder.Register(_ => new[] { 2, 1, 5, 6, 3 }.OrderBy(x => x));
            var container = _builder.Build();

            // Act.
            var resolved = container.Resolve<IOrderedEnumerable<int>>();

            // Assert.
            Assert.Equal(new[] { 1, 2, 3, 5, 6 }, resolved);
        }

        private readonly ContainerBuilder _builder = new ContainerBuilder();

        private class AdditionalMetadata
        {
            public int Data { get; set; }
        }
    }
}
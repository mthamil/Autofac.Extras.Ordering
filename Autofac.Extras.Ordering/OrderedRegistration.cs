using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Metadata;

namespace Autofac.Extras.Ordering
{
    /// <summary>
    /// An Autofac extension that provides control over the order in which multiple dependencies are resolved.
    /// By default, order is not guaranteed when resolving <see cref="IEnumerable{T}"/>. Essentially, a new 
    /// relationship type is supported. Declaring a dependency of type <see cref="IOrderedEnumerable{TElement}"/> 
    /// with this extension allows for a deterministic order.
    /// </summary>
    public static class OrderedRegistration
    {
        /// <summary>
        /// Configures an explicit order that a service should be resolved in.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="order">The order for which a service will be resolved</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OrderBy<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration, int order)
        {
            registration.OrderBy(_ => order);
            return registration;
        }

        /// <summary>
        /// Configures a function that will determine a service's resolution order dynamically.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="keySelector">Selects an ordering based on a component's properties</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> OrderBy<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration, Func<TLimit, IComparable> keySelector)
        {
            registration.WithMetadata<OrderingMetadata<TLimit>>(mc => mc.For(p => p.KeySelector, keySelector));
            return registration;
        }

        /// <summary>
        /// Specifies that a component depends on an ordered collection of services. This is declared by use
        /// of a parameter of type <see cref="IOrderedEnumerable{TElement}"/>.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> UsingOrdering<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration)
        {
            registration.OnPreparing(ConfigureOrderedEnumerableParameter);
            return registration;
        }

        /// <summary>
        /// Specifies that a component depends on an ordered collection of services. This is declared by use
        /// of a parameter of type <see cref="IOrderedEnumerable{TElement}"/>.
        /// </summary>
        /// <param name="registration">Registration to set parameter on.</param>
        public static void UseOrdering(this IComponentRegistration registration)
        {
            registration.Preparing += (o, e) => ConfigureOrderedEnumerableParameter(e);
        }

        private static void ConfigureOrderedEnumerableParameter(PreparingEventArgs e)
        {
            e.Parameters = e.Parameters.Union(new[]
        {
            new ResolvedParameter(
                (p, c) => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>),
                (p, c) => ResolveMethod.MakeGenericMethod(p.ParameterType.GetGenericArguments().Single())
                                       .Invoke(null, new object[] { c }))
        });
        }

        private static object ResolveOrderedEnumerable<TService>(IComponentContext context)
        {
            var registeredType = typeof(IEnumerable<>).MakeGenericType(
                                 typeof(Meta<,>).MakeGenericType(typeof(TService), 
                                                                 typeof(OrderingMetadata<TService>)));
            var resolved = (Meta<TService, OrderingMetadata<TService>>[])context.Resolve(registeredType);
            return new AlreadyOrderedEnumerable<TService>(
                resolved.OrderBy(x => x.Metadata.KeySelector(x.Value))
                        .Select(x => x.Value)
                        .ToArray());
        }

        private static readonly MethodInfo ResolveMethod =
            typeof(OrderedRegistration).GetMethod("ResolveOrderedEnumerable",
                                                   BindingFlags.NonPublic |
                                                   BindingFlags.Static);

        private class AlreadyOrderedEnumerable<T> : IOrderedEnumerable<T>
        {
            public AlreadyOrderedEnumerable(IEnumerable<T> wrapped)
            {
                _wrapped = wrapped;
            }

            /// <summary>
            /// This method would be invoked if OrderBy was called again on the collection (unlikely for this usage).
            /// </summary>
            public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool @descending)
            {
                return @descending
                    ? _wrapped.OrderByDescending(keySelector, comparer)
                    : _wrapped.OrderBy(keySelector, comparer);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _wrapped.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private readonly IEnumerable<T> _wrapped;
        }
    }
}
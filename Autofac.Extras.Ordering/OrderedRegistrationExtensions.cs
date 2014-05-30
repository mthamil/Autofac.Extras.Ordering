using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;

namespace Autofac.Extras.Ordering
{
    /// <summary>
    /// An Autofac extension that provides control over the order in which multiple dependencies are resolved.
    /// By default, order is not guaranteed when resolving <see cref="IEnumerable{T}"/>. Essentially, a new 
    /// relationship type is supported. Declaring a dependency of type <see cref="IOrderedEnumerable{TElement}"/> 
    /// with this extension allows for a deterministic order.
    /// </summary>
    public static class OrderedRegistrationExtensions
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
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration, IComparable order)
        {
            return registration.OrderBy(_ => order);
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
            return registration.WithMetadata(OrderedRegistrationSource.OrderingMetadataKey, keySelector);
        }

        /// <summary>
        /// Configures that services will be resolved in the order in which they are registered.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set parameter on.</param>
        /// <param name="startingWith">An optional starting order</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> OrderByRegistration<TLimit, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration, int startingWith = 1)
        {
            int order = startingWith;
            registration.ActivatorData.ConfigurationActions.Add((type, builder) => 
                builder.OrderBy(order++));
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
        [Obsolete("Use OrderedRegistrationSource.")]
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> UsingOrdering<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration) where TActivatorData : ReflectionActivatorData
        {
            return registration.WithParameter(new OrderedEnumerableParameter());
        }

        /// <summary>
        /// Specifies that a component depends on an ordered collection of services. This is declared by use
        /// of a parameter of type <see cref="IOrderedEnumerable{TElement}"/>.
        /// </summary>
        /// <param name="registration">Registration to set parameter on.</param>
        [Obsolete("Use OrderedRegistrationSource.")]
        public static void UseOrdering(this IComponentRegistration registration)
        {
            registration.Preparing += (o, e) => e.Parameters = e.Parameters.Union(new[] { new OrderedEnumerableParameter() });
        }
    }
}
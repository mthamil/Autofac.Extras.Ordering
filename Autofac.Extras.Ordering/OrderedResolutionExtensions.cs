using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Extras.Ordering.Utilities;
using Autofac.Features.Metadata;

namespace Autofac.Extras.Ordering
{
    /// <summary>
    /// Provides methods that simplify resolution of ordered services.
    /// </summary>
    public static class OrderedResolutionExtensions
    {
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> which is already assumed to be ordered
        /// as an <see cref="IOrderedEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TService">The type of elements</typeparam>
        /// <param name="alreadyOrdered">An already ordered sequence of elements</param>
        /// <returns>The sequence as an <see cref="IOrderedEnumerable{T}"/></returns>
        public static IOrderedEnumerable<TService> AsOrdered<TService>(this IEnumerable<TService> alreadyOrdered)
        {
            return new AlreadyOrderedEnumerable<TService>(alreadyOrdered);
        }

        /// <summary>
        /// Retrieves ordered services from the context.
        /// </summary>
        /// <typeparam name="TService">The type of service to which the results will be cast.</typeparam>
        /// <param name="context">The context from which to resolve the services.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The component instances that provide the service.</returns>
        public static IOrderedEnumerable<TService> ResolveOrdered<TService>(this IComponentContext context, params Parameter[] parameters)
        {
            return context.ResolveOrdered<TService>(parameters.AsEnumerable());
        }

        /// <summary>
        /// Retrieves ordered services from the context.
        /// </summary>
        /// <typeparam name="TService">The type of service to which the results will be cast.</typeparam>
        /// <param name="context">The context from which to resolve the services.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The component instances that provide the service.</returns>
        public static IOrderedEnumerable<TService> ResolveOrdered<TService>(this IComponentContext context, IEnumerable<Parameter> parameters)
        {
            var registeredType = typeof(IEnumerable<>).MakeGenericType(
                                 typeof(Meta<>).MakeGenericType(typeof(TService)));
            var resolved = (Meta<TService>[])context.Resolve(registeredType, parameters);
            return resolved.Where(HasOrderingMetadata)
                           .OrderBy(GetOrderFromMetadata)
                           .Select(t => t.Value)
                           .ToArray()
                           .AsOrdered();
        }

        private static bool HasOrderingMetadata<TService>(Meta<TService> instance)
        {
            return instance.Metadata.ContainsKey(OrderedRegistrationSource.OrderingMetadataKey);
        }

        private static object GetOrderFromMetadata<TService>(Meta<TService> instance)
        {
            var orderingFunction = instance.Metadata[OrderedRegistrationSource.OrderingMetadataKey];
            return ((Delegate)orderingFunction).DynamicInvoke(UnwrapValue(instance.Value));
        }

        private static object UnwrapValue(object value)
        {
            var type = value.GetType();
            if (!IsMetadata(type))
                return value;

            return UnwrapValue(type.GetProperty("Value").GetValue(value)); // Unwrap a layer of metadata.
        }

        private static bool IsMetadata(Type type)
        {
            return type.IsInstanceOfGenericType(typeof(Meta<>)) ||
                   type.IsInstanceOfGenericType(typeof(Meta<,>));
        }

        /// <summary>
        /// A simple wrapper that presents an <see cref="IEnumerable{T}"/> as an assumed-to-already-be-ordered collection.
        /// </summary>
        private sealed class AlreadyOrderedEnumerable<T> : IOrderedEnumerable<T>
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
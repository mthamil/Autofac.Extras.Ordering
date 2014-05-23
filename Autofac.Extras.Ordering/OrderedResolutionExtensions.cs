using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Features.Metadata;

namespace Autofac.Extras.Ordering
{
    /// <summary>
    /// Provides methods that simplify resolution of ordered services.
    /// </summary>
    public static class OrderedResolutionExtensions
    {
        /// <summary>
        /// Retrieves ordered services from the context.
        /// </summary>
        /// <typeparam name="TService">The type of service to which the results will be cast.</typeparam>
        /// <param name="context">The context from which to resolve the services.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The component instances that provide the service.</returns>
        public static IOrderedEnumerable<TService> ResolveOrdered<TService>(this IComponentContext context, params Parameter[] parameters)
        {
            var registeredType = typeof(IEnumerable<>).MakeGenericType(
                                 typeof(Meta<>).MakeGenericType(typeof(TService)));
            var resolved = (Meta<TService>[])context.Resolve(registeredType, parameters);
            return new AlreadyOrderedEnumerable<TService>(
                resolved.OrderBy(GetOrderFromMetadata)
                        .Select(t => t.Value)
                        .ToArray());
        }

        private static object GetOrderFromMetadata<TService>(Meta<TService> instance)
        {
            var orderingFunction = instance.Metadata.Single(m => m.Key == OrderedEnumerableParameter.OrderingMetadataKey);
            return ((Delegate)orderingFunction.Value).DynamicInvoke(instance.Value);
        }

        /// <summary>
        /// A simple wrapper that presents an <see cref="IEnumerable{T}"/> as an assumed-to-already-be-ordered collection.
        /// </summary>
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
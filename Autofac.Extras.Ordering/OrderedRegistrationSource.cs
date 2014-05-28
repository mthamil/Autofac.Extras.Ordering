using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Extras.Ordering.Utilities;

namespace Autofac.Extras.Ordering
{
    /// <summary>
    /// Provides support for <see cref="IOrderedEnumerable{TElement}"/>.
    /// </summary>
    public class OrderedRegistrationSource : IRegistrationSource
    {
        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var typedService = service as IServiceWithType;
            if (typedService != null)
            {
                var serviceType = typedService.ServiceType;
                if (serviceType.IsInstanceOfGenericType(typeof(IOrderedEnumerable<>)))
                {
                    var dependencyType = serviceType.GetGenericArguments().Single();
                    var registration = new ComponentRegistration(
                        Guid.NewGuid(),
                        new DelegateActivator(serviceType, (c, ps) => 
                            ResolveMethod.MakeGenericMethod(dependencyType)
                                         .Invoke(null, new object[] { c, ps })),
                        new CurrentScopeLifetime(),
                        InstanceSharing.None,
                        InstanceOwnership.ExternallyOwned,
                        new[] { service },
                        new Dictionary<string, object>());

                    return new IComponentRegistration[] { registration };
                }
            }

            return Enumerable.Empty<IComponentRegistration>();
        }

        /// <summary>
        /// Gets whether the registrations provided by this source are 1:1 adapters on top
        /// of other components (ie. like Meta, Func, or Owned.)
        /// </summary>
        /// <remarks>Always returns false.</remarks>
        public bool IsAdapterForIndividualComponents { get { return false; } }

        /// <summary>
        /// A description of the registration source.
        /// </summary>
        public override string ToString()
        {
            return OrderedRegistrationSourceResources.OrderedRegistrationSourceDescription;
        }

        private static readonly MethodInfo ResolveMethod =
            typeof(OrderedResolutionExtensions).GetMethod("ResolveOrdered",
                                                          BindingFlags.Public |
                                                          BindingFlags.Static);
    }
}
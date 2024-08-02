namespace CoarseSoftware.Testing.Framework.Core.Proxy
{
    using System.Reflection;
    using System.Security.Cryptography;

    internal class ServiceProviderProxy : DispatchProxy
    {
        private IServiceProvider serviceProvider { get; set; }
        private Type? genericTestExpectationComparerType { get; set; }
        private IEnumerable<Type> explicitTestExpectationComparerTypes { get; set; }
        private IntegrationTestStats.IntegrationStat integrationStat { get; set; }
        private IntegrationTestStats.Invocation invocation {  get; set; }
        private TestRunnerConfiguration configuration { get; set; }
        private IntegrationTestCase.Microservice expectedMicroservice {  get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var serviceType = args[0] as Type;

            var businessService = BusinessService.None;
            if (configuration.Wildcard.ManagerFacetWildCards.Where(w => !string.IsNullOrEmpty(w) && serviceType.FullName.Contains(w)).Any())
            {
                businessService = BusinessService.Manager;
            }
            else if (configuration.Wildcard.EngineFacetWildCards.Where(w => !string.IsNullOrEmpty(w) && serviceType.FullName.Contains(w)).Any())
            {
                businessService = BusinessService.Engine;
            }
            else if (configuration.Wildcard.AccessFacetWildCards.Where(w => !string.IsNullOrEmpty(w) && serviceType.FullName.Contains(w)).Any())
            {
                businessService = BusinessService.Access;
            }
            else if (configuration.Wildcard.ResourceFacetWildCards.Where(w => !string.IsNullOrEmpty(w) && serviceType.FullName.Contains(w)).Any())
            {
                businessService = BusinessService.Resource;
            }
            else if (configuration.Wildcard.UtilityFacetWildCards.Where(w => !string.IsNullOrEmpty(w) && serviceType.FullName.Contains(w)).Any())
            {
                businessService = BusinessService.Utility;
            }

            if (targetMethod.Name != "GetService" || businessService == BusinessService.None)
            {
                var response = targetMethod.Invoke(serviceProvider, args);
                if (response == null)
                {
                    throw new Exception($"No service for typeof {serviceType.FullName}");
                }
                return response;
            }
            
            var registeredType = this.serviceProvider.GetService(serviceType);
            var hasEmptyOrDefaultConstr =
                  registeredType.GetType().GetConstructor(Type.EmptyTypes) != null ||
                  registeredType.GetType().GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .Any(x => x.GetParameters().All(p => p.IsOptional));
            if (businessService == BusinessService.Manager)
            {
                // we use this same proxy, passing in the invocation as the parent
                // if a manager, we don't create a serviceInvocation
                

                // @TODO - with stateful services, the ctor will require a stateId.  StateId should be pulled from the Request wrapper and added.  
                //          cross that bridge when we get there...
                // Note that state should only ever be applied to the manager instance
                var service = hasEmptyOrDefaultConstr
                    ? Activator.CreateInstance(registeredType.GetType())
                    : Activator.CreateInstance(registeredType.GetType(), new[] { this as IServiceProvider });

                integrationStat.MicroserviceInvocation.ServiceType = serviceType.FullName;
                var serviceProxy = IntegratedServiceProxy.Create(service, args[0] as Type, invoc =>
                {
                    integrationStat.MicroserviceInvocation.ServiceType = invoc.ServiceType;
                    integrationStat.MicroserviceInvocation.Method = invoc.Method;
                    integrationStat.MicroserviceInvocation.RequestType = invoc.RequestType;
                    integrationStat.MicroserviceInvocation.ResponseType = invoc.ResponseType;

                }, integrationStat.MicroserviceInvocation, genericTestExpectationComparerType, explicitTestExpectationComparerTypes, expectedMicroservice, configuration);
                return serviceProxy;
            }
            else
            {
                var serviceInvocation = new IntegrationTestStats.Invocation();
                serviceInvocation.ServiceType = serviceType.FullName;
                var serviceProviderProxy = ServiceProviderProxy.Create(this.serviceProvider, serviceInvocation, this.genericTestExpectationComparerType, this.explicitTestExpectationComparerTypes, this.configuration);
                var service = hasEmptyOrDefaultConstr
                    ? registeredType
                    : registeredType.GetType().GetConstructors().Where(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType.Equals(typeof(IServiceProvider))).Any() // at ctor that takes a IServiceProvider
                        ? Activator.CreateInstance(registeredType.GetType(), new[] { serviceProviderProxy as IServiceProvider })
                        : registeredType;

                var serviceProxy = IntegratedServiceProxy.Create(service, args[0] as Type, invoc =>
                {
                    if (this.integrationStat != null)
                    {
                        this.integrationStat.MicroserviceInvocation.ChildInvocations = this.integrationStat.MicroserviceInvocation.ChildInvocations.Append(invoc).ToList();
                    }
                    else
                    {
                        this.invocation.ChildInvocations = this.invocation.ChildInvocations.Append(invoc).ToList();
                    }
                }, serviceInvocation, genericTestExpectationComparerType, explicitTestExpectationComparerTypes, null, configuration);
                return serviceProxy;
            }
        }

        public static object Create(IServiceProvider serviceProvider, IntegrationTestStats.IntegrationStat integrationStat, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes, TestRunnerConfiguration configuration, IntegrationTestCase.Microservice expectedMicroservice)
        {
#if NET8_0
            var proxy = DispatchProxy.Create(typeof(IServiceProvider), typeof(ServiceProviderProxy));
#elif NET6_0
            var dispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(typeof(IServiceProvider), typeof(ServiceProviderProxy));
            var proxy = dispatchProxyCreateMethod.Invoke(null, null);
#endif
            var proxyTypes = (ServiceProviderProxy)proxy;
            proxyTypes.serviceProvider = serviceProvider;
            proxyTypes.integrationStat = integrationStat;
            proxyTypes.genericTestExpectationComparerType = genericTestExpectationComparerType;
            proxyTypes.explicitTestExpectationComparerTypes = explicitTestExpectationComparerTypes;
            proxyTypes.configuration = configuration;
            proxyTypes.expectedMicroservice = expectedMicroservice;
            return proxy;
        }

        public static object Create(IServiceProvider serviceProvider, IntegrationTestStats.Invocation invocation, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes, TestRunnerConfiguration configuration)
        {
#if NET8_0
            var proxy = DispatchProxy.Create(typeof(IServiceProvider), typeof(ServiceProviderProxy));
#elif NET6_0
            var dispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(typeof(IServiceProvider), typeof(ServiceProviderProxy));
            var proxy = dispatchProxyCreateMethod.Invoke(null, null);
#endif
            var proxyTypes = (ServiceProviderProxy)proxy;
            proxyTypes.serviceProvider = serviceProvider;
            proxyTypes.invocation = invocation;
            proxyTypes.genericTestExpectationComparerType = genericTestExpectationComparerType;
            proxyTypes.explicitTestExpectationComparerTypes = explicitTestExpectationComparerTypes;
            proxyTypes.configuration = configuration;
            return proxy;
        }

        public enum BusinessService
        {
            None,
            Manager,
            Engine,
            Access,
            Resource,
            Utility
        }
    }
}

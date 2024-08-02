namespace CoarseSoftware.Testing.Framework.Core.Proxy.Integration
{
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Reflection;

    internal class ServiceCollectionProxy : DispatchProxy
    {
        private IServiceCollection serviceCollection { get; set; }
        private IntegrationTestStats testStats { get; set; }
        private Type? genericTestExpectationComparerType { get; set; }
        private IEnumerable<Type> explicitTestExpectationComparerTypes { get; set; }
        //private string clientName { get; set; }
        //private IntegrationTestCase.Microservice expectedMicroservice { get; set; }
        private IntegrationTestCase integrationTestCase { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var response = targetMethod.Invoke(serviceCollection, args);

            //if (targetMethod.Name == "Add")
            //{
            //    // this is where we check and mock the registration for business services.
            //    return response;
            //}

            return response;
        }

        public IServiceProvider BuildServiceProvider()
        {
            var config = Helpers.GetTestRunnerConfiguration();

            var integrationStat = new IntegrationTestStats.IntegrationStat
            {
                Client = integrationTestCase.Client,
                Description = integrationTestCase.Description,
                MicroserviceInvocation = new IntegrationTestStats.Invocation()
            };
            testStats.IntegrationStats = testStats.IntegrationStats.Append(integrationStat).ToList();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var serviceProviderProxy = ServiceProviderProxy.Create(serviceProvider, integrationStat, genericTestExpectationComparerType, explicitTestExpectationComparerTypes, config, integrationTestCase.Service) as IServiceProvider;
            return serviceProviderProxy;
        }

        public static object Create(IServiceCollection serviceCollection, IntegrationTestStats testStats, IntegrationTestCase integrationTestCase, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes)
        {
#if NET8_0
            var proxy = DispatchProxy.Create(typeof(IServiceCollection), typeof(ServiceCollectionProxy));
#elif NET6_0
            var dispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(typeof(IServiceCollection), typeof(ServiceCollectionProxy));
            var proxy = dispatchProxyCreateMethod.Invoke(null, null);
#endif
            var proxyTypes = (ServiceCollectionProxy)proxy;
            proxyTypes.serviceCollection = serviceCollection;
            proxyTypes.testStats = testStats;
            proxyTypes.genericTestExpectationComparerType = genericTestExpectationComparerType;
            proxyTypes.explicitTestExpectationComparerTypes = explicitTestExpectationComparerTypes;
            proxyTypes.integrationTestCase = integrationTestCase;
            return proxy;
        }
    }
}

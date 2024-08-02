namespace CoarseSoftware.Testing.Framework.Core.Proxy.Client
{
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Reflection;

    internal class ServiceCollectionProxy : DispatchProxy
    {
        private IServiceCollection serviceCollection { get; set; }
        private TestStatStore.ClientTestStat testStats { get; set; }
        private Type? genericTestExpectationComparerType { get; set; }
        private IEnumerable<Type> explicitTestExpectationComparerTypes { get; set; }
        //private string clientName { get; set; }
        //private IntegrationTestCase.Microservice expectedMicroservice { get; set; }
        private ClientTestCase integrationTestCase { get; set; }

        // Requirements
        //  track each service invocation
        //  and optionally record it.  
        //      may be nice to show a sequence diagram.
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var response = targetMethod.Invoke(serviceCollection, args);
            // TODO
            // if this just wrapped the service in a MockServiceProxy, such that we only check for the microservice here, then we dont need to have a IServiceProvider proxy.
            // each invocation reports back their tracking of the sequence.
            // we do need a ServiceProvider proxy to close the child...
            // a service provider, when created for a child service, needs to be another ServiceProviderProxy.
            // Each ServiceProviderProxy reports back up the chain until finished.
            //     report through a delegate
            //     aggregate at each parent
            // The original serviceProviderProxy will get all, which is still tracked as children.
            // it then returns back here, which sets the values on the ClientTestStats




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

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var serviceProviderProxy = ServiceProviderProxy.Create(serviceProvider, genericTestExpectationComparerType, explicitTestExpectationComparerTypes, config, integrationTestCase.Service, testStats) as IServiceProvider;
            return serviceProviderProxy;
        }

        public static object Create(IServiceCollection serviceCollection, TestStatStore.ClientTestStat testStats, ClientTestCase integrationTestCase, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes)
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

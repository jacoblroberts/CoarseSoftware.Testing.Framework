namespace CoarseSoftware.Testing.Framework.Core.Proxy.ClientTest
{
    using System.Reflection;

    internal class ServiceProviderProxy : DispatchProxy
    {
        private IServiceProvider serviceProvider { get; set; }
        private Type? genericTestExpectationComparerType { get; set; }
        private IEnumerable<Type> explicitTestExpectationComparerTypes { get; set; }
        private TestRunnerConfiguration configuration { get; set; }
        private ClientTestCase.Microservice service {  get; set; }
        // append to this... when we ClearServices, it will empty what is here.
        private TestStatStore.ClientTestStat parentStat { get; set; }
        //private ClientTestCase.ClientTestStats.Service activeStat { get; set; }
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {

            // this just gets the service, which should be a proxy wrapper that records and returns.
            //if (targetMethod.DeclaringType.Equals(service.FacetType) && targetMethod.Name == service.ExpectedMethodName)
            //{
            //    // test service.expectedRequest
            //    //      dont unwrap

            //    // return the service.MockResponse
            //}
            if (targetMethod.Name != "GetService")
            {
                return targetMethod.Invoke(serviceProvider, args);
            }

            var requestType = args[0] as Type;
            var registeredType = this.serviceProvider.GetService(requestType);
            var hasEmptyOrDefaultConstr =
                  registeredType.GetType().GetConstructor(Type.EmptyTypes) != null ||
                  registeredType.GetType().GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .Any(x => x.GetParameters().All(p => p.IsOptional));

            //var serviceInvocation = new ClientTestCase.ClientTestStats();
            //serviceInvocation.ServiceType = serviceType.FullName;
            var childStat = new TestStatStore.ClientTestStat();
            var serviceProviderProxy = ServiceProviderProxy.Create(this.serviceProvider, this.genericTestExpectationComparerType, this.explicitTestExpectationComparerTypes, this.configuration, this.service, childStat);
            var service = hasEmptyOrDefaultConstr
                ? registeredType
                // @TODO - we need to be able to use all ctors by getting each arg item type, use Activator to create each item in the ctor.  If an arg is IServiceProvider, we supply it.
                : registeredType.GetType().GetConstructors().Where(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType.Equals(typeof(IServiceProvider))).Any() // at ctor that takes a IServiceProvider
                    ? Activator.CreateInstance(registeredType.GetType(), new[] { serviceProviderProxy as IServiceProvider })
                    : registeredType;

            // the issue is, how do I record from a delegate?
            // when a service is invoked, it could invoke services.  
            // we cant just give it, we have to give it and then get notified when to add it.
            // we need to give it so the child ServiceProvider has something to add to, but it is the same service that is invokeing.
            // 
            // ActiveServiceStat - is created from the MockServiceProxy by invoking a delegate.
            //   when this happens the ServiceProviderProxy hold that currentActiveServiceStat
            //   it uses that currentActiveServiceStat to create the child serviceProviderProxy
            // then invoke a finished delegate, which records the ActiveServiceStat as a child.

            // when the action is invokes from the mock, this will call serviceProviderProxy.ClearInvocations // which return the items cleared.

            var serviceProxy = MockServiceProxy.Create(service, args[0] as Type, (serviceStat) =>
            {
                var childServices = ((ServiceProviderProxy)serviceProviderProxy).ClearServices();
                serviceStat.ChildServices = childServices;
                this.parentStat.Services = this.parentStat.Services.Append(serviceStat);
            }, genericTestExpectationComparerType, explicitTestExpectationComparerTypes, this.service, configuration);
            return serviceProxy;
        }

        public IEnumerable<TestStatStore.ClientTestStat.Service> ClearServices()
        {
            var services = this.parentStat.Services.ToList();
            this.parentStat.Services = new List<TestStatStore.ClientTestStat.Service>();
            return services;
        }

        public static object Create(IServiceProvider serviceProvider, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes, TestRunnerConfiguration configuration, ClientTestCase.Microservice service, TestStatStore.ClientTestStat parentStat)
        {
#if NET8_0
            var proxy = DispatchProxy.Create(typeof(IServiceProvider), typeof(ServiceProviderProxy));
#elif NET6_0
            var dispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(typeof(IServiceProvider), typeof(ServiceProviderProxy));
            var proxy = dispatchProxyCreateMethod.Invoke(null, null);
#endif
            var proxyTypes = (ServiceProviderProxy)proxy;
            proxyTypes.serviceProvider = serviceProvider;
            proxyTypes.genericTestExpectationComparerType = genericTestExpectationComparerType;
            proxyTypes.explicitTestExpectationComparerTypes = explicitTestExpectationComparerTypes;
            proxyTypes.configuration = configuration;
            proxyTypes.service = service;
            proxyTypes.parentStat = parentStat;
            return proxy;
        }
    }
}

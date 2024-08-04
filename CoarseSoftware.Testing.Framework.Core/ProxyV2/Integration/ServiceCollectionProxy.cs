namespace CoarseSoftware.Testing.Framework.Core.ProxyV2.Integration
{
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Reflection;

    internal class ServiceCollectionProxy : DispatchProxy
    {
        private IServiceCollection mockServiceCollection { get; set; }
        private TestRunnerConfiguration configuration { get; set; }
        private IntegrationTestStats testStats { get; set; }
        private Type? genericTestExpectationComparerType { get; set; }
        private IEnumerable<Type> explicitTestExpectationComparerTypes { get; set; }
        private IntegrationTestCase integrationTestCase { get; set; }

        private IntegrationTestStats.IntegrationStat integrationStat { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod.Name == "Add")
            {
                var serviceDescriptor = args[0] as ServiceDescriptor;
                var openChannel = new Action<IntegrationTestStats.Invocation>(invocation =>
                {
                    var channel = this.openInvocationChannel();
                    channel.ServiceType = invocation.ServiceType;
                    channel.Method = invocation.Method;
                    channel.RequestType = invocation.RequestType;
                });
                var closeChannel = new Action<object>(response =>
                {
                    this.closeInvocationChannel(response, integrationStat.MicroserviceInvocation);
                    if (!string.IsNullOrEmpty(integrationStat.MicroserviceInvocation.ResponseType))
                    {
                        // we are done and can compare response
                        Helpers.CompareExpectedToActual(integrationTestCase.Service.ExpectedResponse, response, integrationTestCase.Service.IngoredResponsePropertyNames, explicitTestExpectationComparerTypes, genericTestExpectationComparerType);
                        
                    }
                });

                var getService = new Func<IServiceProvider, object>((sp) =>
                {
                    var registeredImplementationType = serviceDescriptor.ImplementationType;
                    var hasEmptyOrDefaultConstr =
                      registeredImplementationType.GetConstructor(Type.EmptyTypes) != null ||
                      registeredImplementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .Any(x => x.GetParameters().All(p => p.IsOptional));

                    var service = hasEmptyOrDefaultConstr
                        ? Activator.CreateInstance(registeredImplementationType)
                        : Activator.CreateInstance(registeredImplementationType, new[] { sp as IServiceProvider });
                    return service;
                });
                
                var mockServiceDescriptor = new ServiceDescriptor(serviceDescriptor.ServiceType, (sp) =>
                {
                    object actualService = null;
                    if (serviceDescriptor.ImplementationFactory != null)
                    {

                        actualService = serviceDescriptor.ImplementationFactory.Invoke(sp);
                        // reeturn IntegratedServiceProxy.Create 
                    }
                    else
                    {
                        //var actualService = this.mockServiceProvider.GetService(k.GetType());
                        // needs to get the service implementation type, and construct it.
                        actualService = getService(sp);
                    }

                    var mockService = IntegratedServiceProxy.Create(serviceDescriptor.ServiceType, openChannel, closeChannel, actualService, genericTestExpectationComparerType, explicitTestExpectationComparerTypes, configuration);
                    return mockService;
                }, serviceDescriptor.Lifetime);
                
                return targetMethod.Invoke(mockServiceCollection, new[] { mockServiceDescriptor });
            }

            return targetMethod.Invoke(mockServiceCollection, args); ;
        }

        /// <summary>
        /// how this will work is, it will traverse the graph until find a response or there are no children.
        /// </summary>
        /// <returns></returns>
        private IntegrationTestStats.Invocation openInvocationChannel()
        {
            // we run into a problem with parallel tasks
            // one way we can begin to solve this is the open channel will take the caller information and formulate a unique key
            // this breaks the recursion because now we cant infer where the data belongs... we would need to track each.
            //  we could track by 

            // the first one we get should be the microservice
            var invocation = new IntegrationTestStats.Invocation();
            if (integrationStat.MicroserviceInvocation is null)
            {
                integrationStat.MicroserviceInvocation = invocation;
            }
            else
            {
                // traverse to find the child most open channel and append this to the child
                if (!addInvocationChannel(invocation, integrationStat.MicroserviceInvocation))
                {
                    throw new Exception("We should never fail to add");
                }
            }
            return invocation;
        }

        private bool addInvocationChannel(IntegrationTestStats.Invocation invocation, IntegrationTestStats.Invocation parentInvocation)
        {
            foreach(var child in parentInvocation.ChildInvocations)
            {
                if (!string.IsNullOrEmpty(child.ResponseType))
                {
                    // child already closed so we skip
                    continue;
                }

                if (addInvocationChannel(invocation, child))
                {
                    // invocation was added so we are done
                    return true;
                }
            }

            parentInvocation.ChildInvocations = parentInvocation.ChildInvocations.Append(invocation);
            return true;
        }

        //init will pass in the MicroserviceInvocation
        /// <summary>
        /// this will traverse the graph and look for the most child open channel and set the response
        /// </summary>
        private bool closeInvocationChannel(object response, IntegrationTestStats.Invocation invocation)
        {
            foreach(var child in invocation.ChildInvocations)
            {
                if (!string.IsNullOrEmpty(child.ResponseType))
                {
                    // child already closed so we skip
                    continue;
                }

                if (closeInvocationChannel(response, child))
                {
                    // child was closed
                    return true;
                }
                throw new Exception("We should never get here...");
            }

            // if we make it here, we can assume that there are children and they are all close
            invocation.ResponseType = response.GetType().FullName;
            return true;
        }

        public static object Create(IServiceCollection serviceCollection, IntegrationTestStats testStats, IntegrationTestCase integrationTestCase, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes, TestRunnerConfiguration configuration)
        {
#if NET8_0
            var proxy = DispatchProxy.Create(typeof(IServiceCollection), typeof(ServiceCollectionProxy));
#elif NET6_0
            var dispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(typeof(IServiceCollection), typeof(ServiceCollectionProxy));
            var proxy = dispatchProxyCreateMethod.Invoke(null, null);
#endif
            var proxyTypes = (ServiceCollectionProxy)proxy;
            proxyTypes.mockServiceCollection = serviceCollection;
            proxyTypes.testStats = testStats;
            proxyTypes.genericTestExpectationComparerType = genericTestExpectationComparerType;
            proxyTypes.explicitTestExpectationComparerTypes = explicitTestExpectationComparerTypes;
            proxyTypes.integrationTestCase = integrationTestCase;
            proxyTypes.configuration = configuration;

            proxyTypes.integrationStat = new IntegrationTestStats.IntegrationStat
            {
                Client = integrationTestCase.Client,
                Description = integrationTestCase.Description
            };
            proxyTypes.testStats.IntegrationStats = proxyTypes.testStats.IntegrationStats.Append(proxyTypes.integrationStat);
            return proxy;
        }
    }
}

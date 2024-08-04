namespace CoarseSoftware.Testing.Framework.Core.ProxyV2.Integration
{
    using System.Reflection;

    internal class IntegratedServiceProxy : DispatchProxy
    {
        private Type? genericTestExpectationComparerType { get; set; }
        private IEnumerable<Type> explicitTestExpectationComparerTypes { get; set; }

        private Type serviceInterfaceType { get; set; }
        //private Type serviceImplementationType { get; set; }
        private Action<IntegrationTestStats.Invocation> openChannel { get; set; }
        private Action<object> closeChannel { get; set; }
        private object actualService { get; set; }

        private TestRunnerConfiguration configuration;

        /// <summary>
        /// how do we get the service?
        ///    since the collection is wrapping it here, how do we get the actual service?
        ///      2 problems
        ///         1) need to get the second type.  first type is facet, second is the implementation
        ///         2) if there is no implementation T, then there is a delegate used to instantiate the service.
        ///                 we need to invoke that. somehow.    we can keep the delegate here but how do we get a IServiceProvider to pass in?
        ///                         activator.createInstance to get IServiceProvider
        /// </summary>
        /// <param name="targetMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            //Activator.CreateInstance<IServiceProvider>();
         


            object? requestData = null;

            // compare request if manager invocation...
            if (args.Any() && args.First() != null)
            {
                var firstArgType = args.First().GetType();

                var isRequestWrapped = configuration.RequestWrapper != null
                    && firstArgType.IsGenericType
                    && firstArgType.GetGenericTypeDefinition() == configuration.RequestWrapper.OpenWrapperType;

                requestData = isRequestWrapped
                     ? firstArgType.GetProperty(configuration.RequestWrapper.DtoPropertyName).GetValue(args.First())
                     : args.First();

                //if (expectedMicroservice != null)
                //{
                //    var expectedRequestData = isRequestWrapped
                //        ? expectedMicroservice.Request.GetType().GetProperty(configuration.RequestWrapper.DtoPropertyName).GetValue(expectedMicroservice.Request)
                //        : expectedMicroservice.Request;
                //    Helpers.CompareExpectedToActual(expectedRequestData, requestData, expectedMicroservice.IngoredRequestPropertyNames, explicitTestExpectationComparerTypes, genericTestExpectationComparerType);
                //}
            }
            var invocat = new IntegrationTestStats.Invocation
            {
                ServiceType = this.serviceInterfaceType.FullName,
                Method = targetMethod.Name,
                RequestType = requestData != null ? requestData.GetType().FullName : string.Empty,
                //ResponseType = responseData == null || responseData is CoarseSoftware.Testing.Framework.Core.TestCasesRunner.VoidResponse ? string.Empty : responseData.GetType().FullName,
                //ChildInvocations = invocation.ChildInvocations
            };
            this.openChannel(invocat);

            var task = targetMethod.Invoke(actualService, args);
            var isTask = targetMethod.ReturnType.Equals(typeof(Task)) || (targetMethod.ReturnType.IsGenericType && targetMethod.ReturnType.GetGenericTypeDefinition().Equals(typeof(Task<>)));
            object response = null;
            //firstArgType.GetGenericTypeDefinition() == configuration.RequestWrapper.OpenWrapperType;
            if (isTask)
            {
                ((Task)task).Wait();

                var resultProperty = task.GetType().GetProperty("Result");
                response = resultProperty.GetValue(task);
            }
            else
            {
                response = task;
            }

            // i think response could be null for Task/void methods
            object responseData = null;

            if (response != null)
            {
                var responseType = response.GetType();
                var isResponseWrapped = configuration.ResponseWrapper != null
                   && responseType.IsGenericType
                   && responseType.GetGenericTypeDefinition() == configuration.ResponseWrapper.OpenWrapperType;

                responseData = isResponseWrapped
                   ? responseType.GetProperty(configuration.ResponseWrapper.DtoPropertyName).GetValue(response)
                   : response;

                // compare response if manager invocation...
                // we need to know if this is the microservice to run asserts.
                //if (expectedMicroservice != null && expectedMicroservice.ExpectedResponse != null)
                //{
                //    var expectedResponseData = isResponseWrapped
                //        ? expectedMicroservice.ExpectedResponse.GetType().GetProperty(configuration.ResponseWrapper.DtoPropertyName).GetValue(expectedMicroservice.ExpectedResponse)
                //        : expectedMicroservice.ExpectedResponse;

                //    Helpers.CompareExpectedToActual(expectedResponseData, responseData, expectedMicroservice.IngoredResponsePropertyNames, explicitTestExpectationComparerTypes, genericTestExpectationComparerType);
                //}
            }

            // TODO - clean the request and response


            this.closeChannel(response);

            if (isTask)
            {
                var convert_method = typeof(IntegratedServiceProxy).GetMethod("ConvertToTaskHack").MakeGenericMethod(response.GetType());
                var result = convert_method.Invoke(null, new object[] { response });
                return result;
            }
            return response;
        }

        public static Task<T> ConvertToTaskHack<T>(T value)
        {
            return Task.FromResult<T>((T)value);
        }

//        public static object Create(Type serviceInterfaceType, Type serviceImplementationType, Action<IntegrationTestStats.Invocation> openChannel, Action<object> closeChannel, Func<IServiceProvider> getServiceProvider, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes, TestRunnerConfiguration configuration)
//        {
//#if NET8_0
//            var proxy = DispatchProxy.Create(serviceInterfaceType, typeof(IntegratedServiceProxy));
//#elif NET6_0
//            var dispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(serviceType, typeof(IntegratedServiceProxy));
//            var proxy = dispatchProxyCreateMethod.Invoke(null, null);
//#endif
//            var proxyTypes = (IntegratedServiceProxy)proxy;
//            proxyTypes.serviceInterfaceType = serviceInterfaceType;
//            proxyTypes.serviceImplementationType = serviceImplementationType;
//            proxyTypes.openChannel = openChannel;
//            proxyTypes.closeChannel = closeChannel;
//            proxyTypes.getServiceProvider = getServiceProvider;

//            proxyTypes.genericTestExpectationComparerType = genericTestExpectationComparerType;
//            proxyTypes.explicitTestExpectationComparerTypes = explicitTestExpectationComparerTypes;
//            proxyTypes.configuration = configuration;
//            return proxy;
//        }

        public static object Create(
            Type serviceInterfaceType, 
            Action<IntegrationTestStats.Invocation> openChannel, 
            Action<object> closeChannel, 
            object actualService,
            Type? genericTestExpectationComparerType, 
            IEnumerable<Type> explicitTestExpectationComparerTypes, 
            TestRunnerConfiguration configuration)
        {
#if NET8_0
            var proxy = DispatchProxy.Create(serviceInterfaceType, typeof(IntegratedServiceProxy));
#elif NET6_0
            var dispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(serviceInterfaceType, typeof(IntegratedServiceProxy));
            var proxy = dispatchProxyCreateMethod.Invoke(null, null);
#endif
            var proxyTypes = (IntegratedServiceProxy)proxy;
            proxyTypes.serviceInterfaceType = serviceInterfaceType;
            //proxyTypes.serviceImplementationType = serviceImplementationType;
            proxyTypes.openChannel = openChannel;
            proxyTypes.closeChannel = closeChannel;
            proxyTypes.actualService = actualService;

            proxyTypes.genericTestExpectationComparerType = genericTestExpectationComparerType;
            proxyTypes.explicitTestExpectationComparerTypes = explicitTestExpectationComparerTypes;
            proxyTypes.configuration = configuration;
            return proxy;
        }
    }
}

namespace CoarseSoftware.Testing.Framework.Core.Proxy.Integration
{
    using System.Reflection;

    internal class IntegratedServiceProxy : DispatchProxy
    {
        private object service { get; set; }
        private Action<IntegrationTestStats.Invocation> onInvocationComplete { get; set; }
        private Type? genericTestExpectationComparerType { get; set; }
        private IEnumerable<Type> explicitTestExpectationComparerTypes { get; set; }
        private Type serviceType { get; set; }

        /// <summary>
        /// this has a value ONLY if the service is a manager component.
        /// </summary>
        private IntegrationTestCase.Microservice expectedMicroservice { get; set; }
        private TestRunnerConfiguration configuration;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
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
            var task = targetMethod.Invoke(service, args);
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
                if (expectedMicroservice != null && expectedMicroservice.ExpectedResponse != null)
                {
                    var expectedResponseData = isResponseWrapped
                        ? expectedMicroservice.ExpectedResponse.GetType().GetProperty(configuration.ResponseWrapper.DtoPropertyName).GetValue(expectedMicroservice.ExpectedResponse)
                        : expectedMicroservice.ExpectedResponse;

                    Helpers.CompareExpectedToActual(expectedResponseData, responseData, expectedMicroservice.IngoredResponsePropertyNames, explicitTestExpectationComparerTypes, genericTestExpectationComparerType);
                }
            }

            // TODO - clean the request and response
            
            var invocat = new IntegrationTestStats.Invocation
            {
                ServiceType = serviceType.FullName,
                Method = targetMethod.Name,
                RequestType = requestData != null ? requestData.GetType().FullName : string.Empty,
                ResponseType = responseData == null || responseData is CoarseSoftware.Testing.Framework.Core.TestCasesRunner.VoidResponse ? string.Empty : responseData.GetType().FullName,
                //ChildInvocations = invocation.ChildInvocations
            };

            onInvocationComplete.Invoke(invocat);

            if (isTask)
            {
                var responseType = targetMethod.ReturnType.IsGenericType
                    ? targetMethod.ReturnType.GetGenericArguments().First()
                    : response.GetType();

                var convert_method = typeof(IntegratedServiceProxy).GetMethod("ConvertToTaskHack").MakeGenericMethod(responseType);
                var result = convert_method.Invoke(null, new object[] { response });
                return result;
            }
            return response;
        }

        public static Task<T> ConvertToTaskHack<T>(T value)
        {
            return Task.FromResult<T>((T)value);
        }

        public static object Create(object service, Type serviceType, Action<IntegrationTestStats.Invocation> onInvocationComplete, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes, IntegrationTestCase.Microservice expectedMicroservice, TestRunnerConfiguration configuration)
        {
#if NET8_0
            var proxy = DispatchProxy.Create(serviceType, typeof(IntegratedServiceProxy));
#elif NET6_0
            var dispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(serviceType, typeof(IntegratedServiceProxy));
            var proxy = dispatchProxyCreateMethod.Invoke(null, null);
#endif
            var proxyTypes = (IntegratedServiceProxy)proxy;
            proxyTypes.serviceType = serviceType;
            proxyTypes.service = service;
            proxyTypes.onInvocationComplete = onInvocationComplete;
            proxyTypes.genericTestExpectationComparerType = genericTestExpectationComparerType;
            proxyTypes.explicitTestExpectationComparerTypes = explicitTestExpectationComparerTypes;
            //proxyTypes.invocation = invocation;
            proxyTypes.expectedMicroservice = expectedMicroservice;
            proxyTypes.configuration = configuration;
            return proxy;
        }
    }
}

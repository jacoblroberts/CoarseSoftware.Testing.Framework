namespace CoarseSoftware.Testing.Framework.Core
{
    using Microsoft.Extensions.DependencyInjection;
    using CoarseSoftware.Testing.Framework.Core.Proxy;
    using System.Linq;
    using System.Text.Json;
    using CoarseSoftware.Testing.Framework.Core.Comparer;
    using NUnit.Framework;
    using System;
    using System.Threading.Tasks;
    using CoarseSoftware.Testing.Framework.Core.TestCaseProcessor;

    [TestFixture]
    [Ignore("Abstract")]
    public abstract class TestCasesRunner
    {
        [TestCaseSource(nameof(TestCaseData))]
        public async Task<bool> RunTests(TestCaseDataRequestBase testCase, TestStatStore testStatStore)
        {
            switch(testCase)
            {
                case IntegrationTestCaseDataRequest request:
                    {
                        var testResult = await RunIntegrationTestCase(request);
                        if (request.WriteSystemResults != null)
                        {
                            request.WriteSystemResults.Invoke();
                        }
                        return testResult;
                    }
                case TestCaseDataRequest request:
                    return await RunTestCase(request, testStatStore);
                case ClientTestCaseDataRequest request:
                    return await RunClientTestCase(request);
                case GenericClientTestCaseDataRequest request:
                    return await GenericClientTestCaseProcessor.Run(request);
            }
            return false;
        }

        public class IntegrationTestCaseDataRequest: TestCaseDataRequestBase
        {
            public string TestName { get; set; }
            public IServiceCollection ServiceCollection { get; set; }
            public IntegrationTestCase.Microservice ExpectedMicroservice { get; set; }
            public Action? WriteSystemResults { get; set; }
        }

        public class ClientTestCaseDataRequest : TestCaseDataRequestBase
        {
            public string TestName { get; set; }
            public Func<IServiceProvider, object> EntryPoint { get; set; }
            public object ExpectedResponse { set; get; }
            public IEnumerable<string> IngoredExpectedResponsePropertyNames { get; set; }
            public IServiceCollection ServiceCollection { get; set; }
        }

        public class TestCaseDataRequest: TestCaseDataRequestBase
        {
            public string TestName { get; set; }

            public UnitTestCase TestCase { get; set; }
            public IServiceCollection ServiceCollection { get; set; }
            public string Category { get; set; }
            public object? ExpectedResponse { get; set; }
            public IEnumerable<string> IngoredExpectedResponsePropertyNames { get; set; }
        }

        public class GenericClientTestCaseDataRequest: TestCaseDataRequestBase
        {
            public string TestName { get; set; }
            public Func<object> EntryPoint { get; set; }
        }

        public class TestCaseDataRequestBase
        {

        }

        private class CreateTestCaseResponse
        {
            public Guid TestId { get; set; }

            public IEnumerable<PreregisteredDependency> PreregisteredDependencies { get; set; }
            public int DependencyCount { get; set; }

            /// <summary>
            /// This is only nullable to allow for recursion.  Any response outside of a recursive loop is expected to have a value.
            /// </summary>
            public object? ExpectedResponse { get; set; }

            public IEnumerable<string> IngoredExpectedResponsePropertyNames { get; set; }

            public IEnumerable<ForkReason> ForkReasons { get; set; }

            public class ForkReason
            {
                public string Reason { get; set; }
                public ForkOperation Operation { get; set; }
                public class ForkOperation
                {
                    public string TypeName { get; set; }
                    public string Method { get; set; }

                    public string RequestTypeName { get; set; }

                    public string? ResponseTypeName { get; set; }
                }
            }
        }

        private static CreateTestCaseResponse.ForkReason clone(CreateTestCaseResponse.ForkReason forkReason)
        {
            return new CreateTestCaseResponse.ForkReason
            {
                Reason = forkReason.Reason,
                Operation = new CreateTestCaseResponse.ForkReason.ForkOperation
                {
                    TypeName = forkReason.Operation.TypeName,
                    RequestTypeName = forkReason.Operation.RequestTypeName,
                    ResponseTypeName = forkReason.Operation.ResponseTypeName,
                    Method = forkReason.Operation.Method
                }
            };
        }

        private class PreregisteredDependency
        {
            public Type FacetType { get; set; }
            public MockProxy Proxy { get; set; }
        }

        private static IEnumerable<CreateTestCaseResponse> createTestCase(IEnumerable<UnitTestCase.Operation> operations, IEnumerable<PreregisteredDependency> preregisteredDependencies, int dependencyCount, UnitTestCase managerTestCase, IEnumerable<CreateTestCaseResponse.ForkReason> forkReasons, TestStats testStats, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes)
        {
            var config = Helpers.GetTestRunnerConfiguration();
            var expectedResponseFound = false;
            foreach (var operation in operations)
            {
                if (expectedResponseFound)
                {
                    throw new Exception($"No operation should come after a {typeof(UnitTestCase.ExpectedResponseOperation).Name}.");
                }
                switch (operation)
                {
                    case UnitTestCase.ServiceOperation op:
                        {
                            dependencyCount++;

                            var preregisteredDependency = preregisteredDependencies.Where(s => s.FacetType.FullName == op.FacetType.FullName).FirstOrDefault();
                            if (preregisteredDependency == null)
                            {
                                var mockService = MockProxy.Create(op.FacetType, testStats, genericTestExpectationComparerType, explicitTestExpectationComparerTypes, config) as MockProxy;
                                preregisteredDependency = new PreregisteredDependency
                                {
                                    FacetType = op.FacetType,
                                    Proxy = mockService
                                };
                                preregisteredDependencies = preregisteredDependencies.Append(preregisteredDependency).ToList();
                            }
                            switch (op.Response)
                            {
                                case UnitTestCase.Response.ForkingResponse fr:
                                    {
                                        Func<UnitTestCase.Response.ForkingResponse, TestStats, IEnumerable<PreregisteredDependency>, IEnumerable<CreateTestCaseResponse.ForkReason>, int, IEnumerable<CreateTestCaseResponse>, IEnumerable<CreateTestCaseResponse>> forkingHandlerDelegate = null;
                                        forkingHandlerDelegate = new Func<UnitTestCase.Response.ForkingResponse, TestStats, IEnumerable<PreregisteredDependency>, IEnumerable<CreateTestCaseResponse.ForkReason>, int, IEnumerable<CreateTestCaseResponse>, IEnumerable<CreateTestCaseResponse>>((forkingResponse, _testStats, _preregisteredDependencies, _forkReasons, _dependencyCount, existing) =>
                                        {
                                            var copyOfTestStats = _testStats.Clone();
                                            var internalTestStats = _testStats.Clone();

                                            IEnumerable<PreregisteredDependency> copyOfPreregisteredDependencies = _preregisteredDependencies.Select(p => new PreregisteredDependency
                                            {
                                                FacetType = p.FacetType,
                                                Proxy = p.Proxy.Clone(p.FacetType, copyOfTestStats)
                                            }).ToList();
                                            IEnumerable<PreregisteredDependency> internalPreregisteredDependencies = _preregisteredDependencies.Select(p => new PreregisteredDependency
                                            {
                                                FacetType = p.FacetType,
                                                Proxy = p.Proxy.Clone(p.FacetType, internalTestStats)
                                            }).ToList();

                                            IEnumerable<CreateTestCaseResponse.ForkReason> copyOfForkReasons = _forkReasons.Select(fr => clone(fr)).ToList();
                                            IEnumerable<CreateTestCaseResponse.ForkReason> internalForkReasons = _forkReasons.Select(fr => clone(fr)).ToList();

                                            var copyOfDependencyCount = _dependencyCount;
                                            var internalDependencyCount = _dependencyCount;

                                            var internalPreregisteredDependency = internalPreregisteredDependencies.Where(s => s.FacetType.FullName == op.FacetType.FullName).FirstOrDefault();

                                            internalPreregisteredDependency.Proxy.Enqueue(new MockProxy.Response
                                            {
                                                ExpectedInvocationOrder = internalDependencyCount,
                                                Item = forkingResponse.LeftFork.MockResponse,
                                                ExpectedRequest = op.ExpectedRequest,
                                                IngoredPropertyNames = op.IngoredExpectedRequestPropertyNames,
                                                ExpectedMethod = op.MethodName
                                            });

                                            //forkingResponse.LeftFork.Reason
                                            var isMockExpectedRequestWrapped = config.RequestWrapper != null
                                                  && op.ExpectedRequest.GetType().IsGenericType
                                                  && op.ExpectedRequest.GetType().GetGenericTypeDefinition() == config.RequestWrapper.OpenWrapperType;

                                            var requestTypeName = isMockExpectedRequestWrapped
                                                        ? op.ExpectedRequest.GetType().GetProperty(config.RequestWrapper.DtoPropertyName).GetValue(op.ExpectedRequest).GetType().FullName
                                                        : op.ExpectedRequest.GetType().FullName;

                                            var isMockExpectedResponseWrapped = forkingResponse.LeftFork.MockResponse is not null
                                             && config.ResponseWrapper is not null
                                             && forkingResponse.LeftFork.MockResponse.GetType().IsGenericType
                                             && forkingResponse.LeftFork.MockResponse.GetType().GetGenericTypeDefinition() == config.ResponseWrapper.OpenWrapperType;

                                            internalForkReasons = internalForkReasons.Append(new CreateTestCaseResponse.ForkReason
                                            {
                                                Reason = forkingResponse.LeftFork.Reason,
                                                Operation = new CreateTestCaseResponse.ForkReason.ForkOperation
                                                {
                                                    TypeName = op.FacetType.FullName,
                                                    RequestTypeName = requestTypeName,
                                                    ResponseTypeName = forkingResponse.LeftFork.MockResponse is not null
                                                        ? isMockExpectedResponseWrapped
                                                            ? forkingResponse.LeftFork.MockResponse.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(forkingResponse.LeftFork.MockResponse) is not null
                                                                ? forkingResponse.LeftFork.MockResponse.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(forkingResponse.LeftFork.MockResponse).GetType().FullName
                                                                : config.AllowNullResponseData ? "null" : throw new Exception("Null response data is not allowed.  This is bad practice.  To allow null, set RunnerOptions.AllowNullResponseData to true.")
                                                            : forkingResponse.LeftFork.MockResponse.GetType().FullName
                                                        : string.Empty,
                                                    Method = op.MethodName
                                                }
                                            });
                                            var completed = false;
                                            foreach (var v in createTestCase(forkingResponse.LeftFork.Operations, internalPreregisteredDependencies, internalDependencyCount, managerTestCase, internalForkReasons.ToList(), internalTestStats, genericTestExpectationComparerType, explicitTestExpectationComparerTypes))
                                            {
                                                if (completed)
                                                {
                                                    throw new Exception("Only the last response will have a null TestCaseData");
                                                }

                                                if (v.ExpectedResponse != null)
                                                {
                                                    //yield return v;
                                                    existing = existing.Append(v);
                                                }
                                                else
                                                {
                                                    internalDependencyCount = v.DependencyCount;
                                                    internalPreregisteredDependencies = v.PreregisteredDependencies;
                                                    //forkReasons = v.ForkReasons;
                                                    completed = true;
                                                }
                                            }

                                            // handle the RightFork
                                            // if the right fork is a MockResponse, we can handle it here
                                            //      if it is a ForkingResponse, we need to invoke create.
                                            switch (forkingResponse.RightFork.Response)
                                            {
                                                case UnitTestCase.Response.ForkingResponse fork:
                                                    {
                                                        var recursiveResponse = forkingHandlerDelegate.Invoke(fork, copyOfTestStats, copyOfPreregisteredDependencies, copyOfForkReasons, copyOfDependencyCount, new List<CreateTestCaseResponse>());
                                                        existing = existing.Concat(recursiveResponse);
                                                        break;
                                                    }
                                                case UnitTestCase.Response.MockResponse mock:
                                                    {
                                                        var preregisteredDependencyFromCopy = copyOfPreregisteredDependencies.Where(s => s.FacetType.FullName == op.FacetType.FullName).FirstOrDefault();
                                                        if (preregisteredDependencyFromCopy == null)
                                                        {
                                                            throw new Exception("this should already be set before the copy");
                                                        }
                                                        preregisteredDependencyFromCopy.Proxy.Enqueue(new MockProxy.Response
                                                        {
                                                            ExpectedInvocationOrder = copyOfDependencyCount,
                                                            Item = mock.Response,
                                                            ExpectedRequest = op.ExpectedRequest,
                                                            IngoredPropertyNames = op.IngoredExpectedRequestPropertyNames,
                                                            ExpectedMethod = op.MethodName
                                                        });

                                                        var rightForkResponse = forkingResponse.RightFork.Response is UnitTestCase.Response.MockResponse
                                                            ? ((UnitTestCase.Response.MockResponse)forkingResponse.RightFork.Response).Response
                                                            : null;

                                                        var isRightMockExpectedResponseWrapped = rightForkResponse is not null
                                                         && config.ResponseWrapper is not null
                                                         && rightForkResponse.GetType().IsGenericType
                                                         && rightForkResponse.GetType().GetGenericTypeDefinition() == config.ResponseWrapper.OpenWrapperType;

                                                        copyOfForkReasons = copyOfForkReasons.Append(new CreateTestCaseResponse.ForkReason
                                                        {
                                                            Reason = forkingResponse.RightFork.Reason,
                                                            Operation = new CreateTestCaseResponse.ForkReason.ForkOperation
                                                            {
                                                                TypeName = op.FacetType.FullName,
                                                                RequestTypeName = requestTypeName,
                                                                ResponseTypeName = rightForkResponse is not null
                                                                    ? isRightMockExpectedResponseWrapped
                                                                        ? rightForkResponse.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(rightForkResponse) is not null
                                                                            ? rightForkResponse.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(rightForkResponse).GetType().FullName
                                                                            : config.AllowNullResponseData ? "null" : throw new Exception("Null response data is not allowed.  This is bad practice.  To allow null, set RunnerOptions.AllowNullResponseData to true.")
                                                                        : rightForkResponse.GetType().FullName
                                                                    : string.Empty,
                                                                Method = op.MethodName
                                                            }
                                                        });

                                                        //copyOfForkReasons = copyOfForkReasons.Append(forkingResponse.RightFork.Reason);
                                                        completed = false;
                                                        foreach (var v in createTestCase(forkingResponse.RightFork.Operations, copyOfPreregisteredDependencies, copyOfDependencyCount, managerTestCase, copyOfForkReasons.ToList(), copyOfTestStats, genericTestExpectationComparerType, explicitTestExpectationComparerTypes))
                                                        {
                                                            if (completed)
                                                            {
                                                                throw new Exception("Only the last response will have a null TestCaseData");
                                                            }

                                                            if (v.ExpectedResponse != null)
                                                            {
                                                                //yield return v;
                                                                existing = existing.Append(v);
                                                            }
                                                            else
                                                            {
                                                                //set the cound and serviceCollection
                                                                copyOfDependencyCount = v.DependencyCount;
                                                                copyOfPreregisteredDependencies = v.PreregisteredDependencies;
                                                                //copyOfForkReasons = v.ForkReasons;
                                                                completed = true;
                                                            }
                                                        }
                                                        break;
                                                    }

                                            }
                                           
                                            return existing;

                                            
                                        });
                                        //need to create copies first because we run the left side first, which could change the dependencies
                                        foreach(var resp in forkingHandlerDelegate.Invoke(fr, testStats, preregisteredDependencies, forkReasons, dependencyCount, new List<CreateTestCaseResponse>()))
                                        {
                                            yield return resp;
                                        }

                                        preregisteredDependency.Proxy.Enqueue(new MockProxy.Response
                                        {
                                            ExpectedInvocationOrder = dependencyCount,
                                            Item = fr.LeftFork.MockResponse,
                                            ExpectedRequest = op.ExpectedRequest,
                                            IngoredPropertyNames = op.IngoredExpectedRequestPropertyNames,
                                            ExpectedMethod = op.MethodName
                                        });

                                        break;
                                    }
                                case UnitTestCase.Response.MockResponse mockResponse:
                                    {
                                        preregisteredDependency.Proxy.Enqueue(new MockProxy.Response
                                        {
                                            ExpectedInvocationOrder = dependencyCount,
                                            Item = mockResponse.Response,
                                            ExpectedRequest = op.ExpectedRequest,
                                            IngoredPropertyNames = op.IngoredExpectedRequestPropertyNames,
                                            ExpectedMethod = op.MethodName
                                        });
                                        break;
                                    }
                                default:
                                    {
                                        if (op.Response != null)
                                        {
                                            throw new Exception($"Unhandled switch for typeof: {op.Response.GetType().FullName}");
                                        }
                                        preregisteredDependency.Proxy.Enqueue(new MockProxy.Response
                                        {
                                            ExpectedInvocationOrder = dependencyCount,
                                            Item = null,
                                            ExpectedRequest = op.ExpectedRequest,
                                            IngoredPropertyNames = op.IngoredExpectedRequestPropertyNames,
                                            ExpectedMethod = op.MethodName
                                        });
                                        break;
                                    }
                            }


                            break;
                        }
                    case UnitTestCase.ExpectedResponseOperation op:
                        {
                            expectedResponseFound = true;

                            yield return new CreateTestCaseResponse
                            {
                                TestId = op.Id,
                                DependencyCount = dependencyCount,
                                PreregisteredDependencies = preregisteredDependencies,
                                ExpectedResponse = op.ExpectedResponse ?? new VoidResponse(),
                                ForkReasons = forkReasons.ToList(),
                                IngoredExpectedResponsePropertyNames = op.IngoredExpectedResponsePropertyNames
                            };
                            break;
                        }
                }
            }

            // once we get through all operations, there may have not been a ExpectedResponseOperation.
            // we still need to return a value
            if (!expectedResponseFound)
            {
                yield return new CreateTestCaseResponse
                {
                    DependencyCount = dependencyCount,
                    PreregisteredDependencies = preregisteredDependencies,
                    ForkReasons = forkReasons.ToList()
                };
            }
        }

        /// <summary>
        /// Used by explicit tests
        /// </summary>
        /// <param name="testCases"></param>
        /// <returns></returns>
        public static Task<bool[]> RunTestCases(IEnumerable<UnitTestCase> testCases)
        {
            // invoke buildTestCaseDataRequests
            // run test per item
            //foreach (var testCaseDataRequest in buildTestCaseDataRequests(testCases))
            //{

            //    yield return await runTestCase(testCaseDataRequest);
            //}

            return Task.WhenAll(
                buildTestCaseDataRequests(testCases).Select(testCaseDataRequest => RunTestCase(testCaseDataRequest))
            );
        }

        public static async Task<bool> RunTestCase(TestCaseDataRequest testCaseDataRequest, TestStatStore? testStatStore = null)
        {
            var conceptInterfaceTypeName = testCaseDataRequest.TestCase.ConceptInterfaceType.FullName;

            // build the service provider
            var serviceProvider = testCaseDataRequest.ServiceCollection.BuildServiceProvider();
            var componentService = testCaseDataRequest.ServiceCollection.Where(serviceDescriptor =>
            {
                return serviceDescriptor.ServiceType.FullName == conceptInterfaceTypeName;
            }).First();

            var method = componentService.ServiceType.GetMethods().Where(m => m.Name == testCaseDataRequest.TestCase.Method).First();

            var facetMethod = method;

            // invoke operation
            var manager = serviceProvider.GetService(componentService.ServiceType);
            var task = (Task)facetMethod.Invoke(manager, new object[] { testCaseDataRequest.TestCase.Request, CancellationToken.None });
            await task.ConfigureAwait(false);

            // get response
            var resultProperty = task.GetType().GetProperty("Result");
            var response = resultProperty.GetValue(task);
            if (response.GetType().FullName != "System.Threading.Tasks.VoidTaskResult")
            {
                var responseData = response.GetType().GetProperty("Data").GetValue(response);

                var expectedResponseData = testCaseDataRequest.ExpectedResponse.GetType().GetProperty("Data").GetValue(testCaseDataRequest.ExpectedResponse);

                Assert.AreEqual(expectedResponseData.GetType().FullName, responseData.GetType().FullName);

                // running compare logic
                var comparerTypes = getComparerTypes();
                var explicitTestExpectationComparer = comparerTypes.Item2.Where(i => i.GetGenericArguments()[0] == expectedResponseData.GetType()).SingleOrDefault();
                if (explicitTestExpectationComparer != null)
                {
                    // an explicit comparer was found for the response so we use that to compere
                    var explicitTestInstance = Activator.CreateInstance(explicitTestExpectationComparer);
                    var explicitCompareMethod = explicitTestExpectationComparer.GetMethod("Compare");
                    explicitCompareMethod.Invoke(explicitTestInstance,
                        new[] { expectedResponseData, responseData, testCaseDataRequest.IngoredExpectedResponsePropertyNames });
                }
                else
                {
                    // runs the generic test comparer wich does a deep compare of each property to property
                    var genericTestInstance = Activator.CreateInstance(comparerTypes.Item1);
                    var genericCompareMethod = comparerTypes.Item1.GetMethod("Compare");
                    genericCompareMethod.Invoke(genericTestInstance,
                        new[] { expectedResponseData, responseData, testCaseDataRequest.IngoredExpectedResponsePropertyNames });
                }
            }
            else
            {
                Assert.True(testCaseDataRequest.ExpectedResponse is VoidResponse);
            }

            if (testStatStore != null)
            {
                // @TODO - set the request type and response type, if not in the list in testStatStore.
                // used to click through use cases in the call chain
            }

            return true;
        }

        private static IEnumerable<TestCaseDataRequest> buildTestCaseDataRequests(IEnumerable<UnitTestCase> testCases)
        {
            foreach (var testCase in testCases)
            {
                var testCaseDatas = buildTestCaseData(
                    testCase,
                    new TestStats(),
                    new TestStatStore(),
                    null,
                    new List<Type>()
                );

                foreach (var testCaseData in testCaseDatas)
                {
                    yield return testCaseData.Arguments[0] as TestCaseDataRequest;
                }
            }
        }


        ////// Integration Tests
        public static async Task<bool> RunIntegrationTestCase(IntegrationTestCaseDataRequest testCase)
        {
            var serviceProvider = testCase.ServiceCollection.BuildServiceProvider();
            var microservice = serviceProvider.GetRequiredService(testCase.ExpectedMicroservice.FacetType);
            var facetMethod = testCase.ExpectedMicroservice.FacetType.GetMethod(testCase.ExpectedMicroservice.MethodName);
            var task = (Task)facetMethod.Invoke(microservice, new object[] { testCase.ExpectedMicroservice.Request, CancellationToken.None });
            await task.ConfigureAwait(false);

            // get response
            var resultProperty = task.GetType().GetProperty("Result");
            var response = resultProperty.GetValue(task);

            var comparerTypes = getComparerTypes();
            Type? genericTestModelComparerType = comparerTypes.Item1; //ITestModelComparer
            var explicitTestModelComparerTypess = comparerTypes.Item2; //ITestModelComparer<T>

            // compare
            Helpers.CompareExpectedToActual(testCase.ExpectedMicroservice.ExpectedResponse, response, testCase.ExpectedMicroservice.IngoredResponsePropertyNames, explicitTestModelComparerTypess, genericTestModelComparerType);

            return true;
        }

        ////// Client Tests
        public static async Task<bool> RunClientTestCase(ClientTestCaseDataRequest testCase)
        {
            var serviceProvider = (testCase.ServiceCollection as CoarseSoftware.Testing.Framework.Core.Proxy.Client.ServiceCollectionProxy).BuildServiceProvider();
            var entryPointResponse = testCase.EntryPoint.Invoke(serviceProvider);
            // compare response to testCase.ExpectedResponse

            var comparerTypes = getComparerTypes();
            Type? genericTestModelComparerType = comparerTypes.Item1; //ITestModelComparer
            var explicitTestModelComparerTypess = comparerTypes.Item2; //ITestModelComparer<T>

            //var unwrappedTaskResponse = 
            var responseType = entryPointResponse.GetType();
            var isTask = responseType.Equals(typeof(Task)) || (responseType.IsGenericType && responseType.GetGenericTypeDefinition().Equals(typeof(Task<>)));
            object response = null;
            //firstArgType.GetGenericTypeDefinition() == configuration.RequestWrapper.OpenWrapperType;
            if (isTask)
            {
                ((Task)entryPointResponse).Wait();

                var resultProperty = responseType.GetProperty("Result");
                response = resultProperty.GetValue(entryPointResponse);
            }
            else
            {
                response = entryPointResponse;
            }

            var expectedResponseType = testCase.ExpectedResponse.GetType();
            isTask = expectedResponseType.Equals(typeof(Task)) || (expectedResponseType.IsGenericType && expectedResponseType.GetGenericTypeDefinition().Equals(typeof(Task<>)));
            object expectedResponse = null;
            //firstArgType.GetGenericTypeDefinition() == configuration.RequestWrapper.OpenWrapperType;
            if (isTask)
            {
                ((Task)testCase.ExpectedResponse).Wait();

                var resultProperty = expectedResponseType.GetProperty("Result");
                expectedResponse = resultProperty.GetValue(testCase.ExpectedResponse);
            }
            else
            {
                expectedResponse = testCase.ExpectedResponse;
            }

            // compare
            Helpers.CompareExpectedToActual(expectedResponse, response, testCase.IngoredExpectedResponsePropertyNames, explicitTestModelComparerTypess, genericTestModelComparerType);

            return true;
        }

        public static TestCaseData buildClientTestCaseData(ClientTestCase integrationTestCase, TestStatStore.ClientTestStat testStats, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes)
        {
            var config = Helpers.GetTestRunnerConfiguration();

            var serviceCollection = new ServiceCollection();
            var serviceCollectionProxy = CoarseSoftware.Testing.Framework.Core.Proxy.Client.ServiceCollectionProxy.Create(serviceCollection, testStats, integrationTestCase, genericTestExpectationComparerType, explicitTestExpectationComparerTypes) as IServiceCollection;
            integrationTestCase.ServiceRegistration?.Invoke(serviceCollectionProxy);

            // from integrationTestCase.ExpectedMicroservice.FacetType, get the concept
            //var concept = integrationTestCase.ExpectedMicroservice.FacetType.FullName.Split(".")[^4];
            // from integrationTestCase.ExpectedMicroservice.FacetType, get the volatility
            var volatility = integrationTestCase.Service.FacetType.FullName.Split(".")[^3];
            // from integrationTestCase.ExpectedMicroservice.Request, get the context

            // from integrationTestCase.ExpectedMicroservice.Response, get the class name.
            var isResponseWrapped = config.ResponseWrapper != null
                && integrationTestCase.Service.MockResponse.GetType().IsGenericType
                && integrationTestCase.Service.MockResponse.GetType().GetGenericTypeDefinition() == config.ResponseWrapper.OpenWrapperType;

            var responseClass = isResponseWrapped
                ? integrationTestCase.Service.MockResponse.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(integrationTestCase.Service.MockResponse).GetType().Name
                : integrationTestCase.Service.MockResponse.GetType().Name;

            // from integrationTestCase.ExpectedMicroservice.Request, get the class name
            var isRequestWrapped = config.RequestWrapper != null
            && integrationTestCase.Service.ExpectedRequest.GetType().IsGenericType
             && integrationTestCase.Service.ExpectedRequest.GetType().GetGenericTypeDefinition() == config.RequestWrapper.OpenWrapperType;

            var requestClass = isRequestWrapped
                ? integrationTestCase.Service.ExpectedRequest.GetType().GetProperty(config.RequestWrapper.DtoPropertyName).GetValue(integrationTestCase.Service.ExpectedRequest).GetType().Name
                : integrationTestCase.Service.ExpectedRequest.GetType().Name;

            var requestType = isRequestWrapped ? integrationTestCase.Service.ExpectedRequest.GetType().GetProperty(config.RequestWrapper.DtoPropertyName).GetValue(integrationTestCase.Service.ExpectedRequest).GetType() : integrationTestCase.Service.ExpectedRequest.GetType();
            var context = requestType.Namespace.Contains("UseCases.") ? requestType.Namespace.Substring(requestType.Namespace.IndexOf("UseCases.")).Replace("UseCases.", "") : "Default";

            var testCaseDataRequest = new ClientTestCaseDataRequest
            {
                TestName = $"Client.{integrationTestCase.Client}.{volatility}.{context}.{requestClass}.{responseClass}.{integrationTestCase.Id} | {integrationTestCase.Description}",
                ServiceCollection = serviceCollectionProxy,
                EntryPoint = integrationTestCase.EntryPoint,
                ExpectedResponse = integrationTestCase.ExpectedResponse,
                IngoredExpectedResponsePropertyNames = integrationTestCase.IngoredExpectedResponsePropertyNames ?? new List<string> { }
            };

            var testCaseData = new TestCaseData(testCaseDataRequest, new TestStatStore());

            testCaseData.SetCategory($"Client.{integrationTestCase.Client}.{volatility}.{context}.{requestClass}.{responseClass}"); //$"Client: {integrationTestCase.Client}");
            testCaseData.SetName(testCaseDataRequest.TestName);
            testCaseData.Returns(true);

            return testCaseData;
        }

        public static TestCaseData buildIntegrationTestCaseData(IntegrationTestCase integrationTestCase, IntegrationTestStats testStats, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes, Action writeSystemResults = null)
        {
            var config = Helpers.GetTestRunnerConfiguration();

            var serviceCollection = new ServiceCollection();
            var serviceCollectionProxy = CoarseSoftware.Testing.Framework.Core.ProxyV2.Integration.ServiceCollectionProxy.Create(serviceCollection, testStats, integrationTestCase, genericTestExpectationComparerType, explicitTestExpectationComparerTypes, config) as IServiceCollection;
            integrationTestCase.ServiceRegistration?.Invoke(serviceCollectionProxy);

            // from integrationTestCase.ExpectedMicroservice.FacetType, get the concept
            //var concept = integrationTestCase.ExpectedMicroservice.FacetType.FullName.Split(".")[^4];
            // from integrationTestCase.ExpectedMicroservice.FacetType, get the volatility
            var volatility = integrationTestCase.Service.FacetType.FullName.Split(".")[^3];
            // from integrationTestCase.ExpectedMicroservice.Request, get the context
            
            // from integrationTestCase.ExpectedMicroservice.Response, get the class name.
            var isResponseWrapped = config.ResponseWrapper != null
                && integrationTestCase.Service.ExpectedResponse.GetType().IsGenericType
                && integrationTestCase.Service.ExpectedResponse.GetType().GetGenericTypeDefinition() == config.ResponseWrapper.OpenWrapperType;

            var responseClass = isResponseWrapped
                ? integrationTestCase.Service.ExpectedResponse.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(integrationTestCase.Service.ExpectedResponse).GetType().Name
                : integrationTestCase.Service.ExpectedResponse.GetType().Name;

            // from integrationTestCase.ExpectedMicroservice.Request, get the class name
            var isRequestWrapped = 
                config.RequestWrapper != null
                && integrationTestCase.Service.Request.GetType().IsGenericType
                && integrationTestCase.Service.Request.GetType().GetGenericTypeDefinition() == config.RequestWrapper.OpenWrapperType;

            var requestClass = isRequestWrapped
                ? integrationTestCase.Service.Request.GetType().GetProperty(config.RequestWrapper.DtoPropertyName).GetValue(integrationTestCase.Service.Request).GetType().Name
                : integrationTestCase.Service.Request.GetType().Name;

            var requestType = isRequestWrapped ? integrationTestCase.Service.Request.GetType().GetProperty(config.RequestWrapper.DtoPropertyName).GetValue(integrationTestCase.Service.Request).GetType() : integrationTestCase.Service.Request.GetType();
            var context = requestType.Namespace.Contains("UseCases.") ? requestType.Namespace.Substring(requestType.Namespace.IndexOf("UseCases.")).Replace("UseCases.", "") : "Default";

            var testCaseDataRequest = new IntegrationTestCaseDataRequest
            {
                //TestName = $"Integration Test - {integrationTestCase.ExpectedMicroservice.FacetType.FullName} ID: {integrationTestCase.Id}",
                TestName = $"Integration.{integrationTestCase.Client}.{volatility}.{context}.{requestClass}.{responseClass}.{integrationTestCase.Id} | {integrationTestCase.Description}",
                ServiceCollection = serviceCollectionProxy,
                ExpectedMicroservice = integrationTestCase.Service,
                //ExpectedResponse = integrationTestCase.ExpectedResponse,
                //IngoredExpectedResponsePropertyNames = integrationTestCase.IngoredExpectedResponsePropertyNames ?? new List<string> { },
                WriteSystemResults = writeSystemResults
            };

            var testCaseData = new TestCaseData(testCaseDataRequest, new TestStatStore());

            testCaseData.SetCategory($"Integration.{integrationTestCase.Client}.{volatility}.{context}.{requestClass}.{responseClass}"); //$"Client: {integrationTestCase.Client}");
            testCaseData.SetName(testCaseDataRequest.TestName);
            testCaseData.Returns(true);

            return testCaseData;
        }



        /// <summary>
        /// the caller will iterate the tuples, using service to invoke hosting.  it will return the IServiceProvider
        /// </summary>
        /// <param name="managerTestCase"></param>
        /// <returns></returns>
        public static IEnumerable<TestCaseData> buildTestCaseData(UnitTestCase testCaseToBuild, TestStats testStats, TestStatStore testStatStore, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes)
        {
            var config = Helpers.GetTestRunnerConfiguration();

            var isRequestWrapped = config.RequestWrapper != null
                  && testCaseToBuild.Request.GetType().IsGenericType
                  && testCaseToBuild.Request.GetType().GetGenericTypeDefinition() == config.RequestWrapper.OpenWrapperType;

            var requestType = isRequestWrapped ? testCaseToBuild.Request.GetType().GetProperty(config.RequestWrapper.DtoPropertyName).GetValue(testCaseToBuild.Request).GetType() : testCaseToBuild.Request.GetType();
            var context = requestType.Namespace.Contains("UseCases.") ? requestType.Namespace.Substring(requestType.Namespace.IndexOf("UseCases.")).Replace("UseCases.", "") : "Default";

            var createTestCaseData = new Func<UnitTestCase, IServiceCollection, CreateTestCaseResponse, TestCaseData>((managerTestCase, serviceCollection, createTestCaseResponse) =>
            {
                var isResponseWrapped = config.ResponseWrapper != null
                  && createTestCaseResponse.ExpectedResponse.GetType().IsGenericType
                  && createTestCaseResponse.ExpectedResponse.GetType().GetGenericTypeDefinition() == config.ResponseWrapper.OpenWrapperType;

                var expectedResponseData = createTestCaseResponse.ExpectedResponse is not VoidResponse 
                    ? isResponseWrapped
                        ? createTestCaseResponse.ExpectedResponse.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(createTestCaseResponse.ExpectedResponse) 
                        : createTestCaseResponse.ExpectedResponse
                    : typeof(VoidResponse);

                

                // from managerTestCase.ConceptInterfaceType, get concept
                //  Biz.Concept.Volatility.Aspect.IService
                var concept = managerTestCase.ConceptInterfaceType.FullName.Split(".")[^4]; ;
                // from managerTestCase.ConceptInterfaceType, get volatility
                var volatility = managerTestCase.ConceptInterfaceType.FullName.Split(".")[^3];

                // from managerTestCase.Request, get context
                var testCaseContext = context;// managerTestCase.Request.GetType().Namespace.Contains("UseCases.") ? managerTestCase.Request.GetType().Namespace.Substring(managerTestCase.Request.GetType().Namespace.IndexOf("UseCases.")).Replace("UseCases.", "") : "Default";
                // fromg managerTestCase.Request, get class name
                var isRequestWrapped = config.RequestWrapper != null
                 && managerTestCase.Request.GetType().IsGenericType
                  && managerTestCase.Request.GetType().GetGenericTypeDefinition() == config.RequestWrapper.OpenWrapperType;

                var requestClass = isRequestWrapped
                    ? managerTestCase.Request.GetType().GetProperty(config.RequestWrapper.DtoPropertyName).GetValue(managerTestCase.Request).GetType().Name
                    : managerTestCase.Request.GetType().Name;

                // from expectedResponseData, get class name
                var responseClass = expectedResponseData.GetType().Name;
                var conditions = string.Join(" & ", createTestCaseResponse.ForkReasons.Select(fr => fr.Reason));
                var testName = $"Unit.{volatility}{concept}.{testCaseContext}.{requestClass}.{responseClass}.{createTestCaseResponse.TestId} | {managerTestCase.Description}";
                if (!string.IsNullOrEmpty(conditions))
                {
                    testName = $"{testName} | Conditions: {conditions}";
                }
                var testCaseDataRequest = new TestCaseDataRequest
                {
                    TestCase = managerTestCase,
                    ServiceCollection = serviceCollection,
                    Category = $"Manager.{managerTestCase.ConceptInterfaceType}.{context}.{requestType.Name}.{expectedResponseData.GetType().Name}",
                    ExpectedResponse = createTestCaseResponse.ExpectedResponse,
                    //TestName = createTestCaseResponse.ForkReasons.Any()
                    //    ? $"{managerTestCase.ConceptInterfaceType}.UseCases.{context}.{expectedResponseData.GetType().Name} Under Condition; {string.Join(" & ", createTestCaseResponse.ForkReasons.Select(fr => fr.Reason))}  ID: {createTestCaseResponse.TestId}"
                    //    : $"{managerTestCase.ConceptInterfaceType}.UseCases.{context}.{expectedResponseData.GetType().Name} ID: {createTestCaseResponse.TestId}",
                    TestName = testName,
                    IngoredExpectedResponsePropertyNames = createTestCaseResponse.IngoredExpectedResponsePropertyNames
                };

                var testCaseData = new TestCaseData(testCaseDataRequest, testStatStore);
                
                testCaseData.SetCategory($"Unit.{volatility}{concept}.{testCaseContext}.{requestClass}.{responseClass}"); //$"Context: {context} | {managerTestCase.Description}");
                testCaseData.SetName(testCaseDataRequest.TestName);
                testCaseData.Returns(true);

                // now set the UseCaseTracking on the TestStatStore
                //var operations = new List<TestStatStore.UseCaseTracking.Operation>();
                var testId = string.Empty;
                //foreach (var prd in createTestCaseResponse.PreregisteredDependencies)
                var operations = getOperationsForTestCase(createTestCaseResponse.TestId, managerTestCase.Operations, config);
                //foreach(var op in operationsForTestCase)
                //{
                //    if (op is not TestCase.ServiceOperation)
                //    {
                //        continue;
                //    }
                //    var operation = op as TestCase.ServiceOperation;
                //    //var index = operations.Where(o => o.TypeName == prd.FacetType.FullName).Count();
                //    //var mockResponse = prd.Proxy.OrderedInvocations.ElementAt(index);
                    
                //    var isMockExpectedRequestWrapped = config.RequestWrapper != null
                //      && operation.ExpectedRequest.GetType().IsGenericType
                //      && operation.ExpectedRequest.GetType().GetGenericTypeDefinition() == config.RequestWrapper.OpenWrapperType;

                //    var isMockExpectedResponseWrapped = operation.Res.Item is not null 
                //     && config.ResponseWrapper != null
                //     && mockResponse.Item.GetType().IsGenericType
                //     && mockResponse.Item.GetType().GetGenericTypeDefinition() == config.ResponseWrapper.OpenWrapperType;

                //    operations.Add(new TestStatStore.UseCaseTracking.Operation
                //    {
                //        TypeName = prd.FacetType.FullName,
                //        Method = mockResponse.ExpectedMethod,
                //        RequestTypeName = isMockExpectedRequestWrapped
                //            ? mockResponse.ExpectedRequest.GetType().GetProperty(config.RequestWrapper.DtoPropertyName).GetValue(mockResponse.ExpectedRequest).GetType().FullName 
                //            : mockResponse.ExpectedRequest.GetType().FullName,
                //        ResponseTypeName = mockResponse.Item is not null
                //            ? isMockExpectedResponseWrapped
                //                ? mockResponse.Item.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(mockResponse.Item) is not null 
                //                    ? mockResponse.Item.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(mockResponse.Item).GetType().FullName 
                //                    : config.AllowNullResponseData ? string.Empty : throw new Exception("Null response data is not allowed.  This is bad practice.  To allow null, set RunnerOptions.AllowNullResponseData to true.")
                //                : mockResponse.Item.GetType().FullName
                //            : string.Empty
                //    });
                //}

                var isUtility = config.Wildcard.UtilityFacetWildCards.Where(c => managerTestCase.ConceptInterfaceType.FullName.Contains(c)).Any();
                if (!isUtility) //".iFX."))
                {
                    testStatStore.UseCaseTrackings = testStatStore.UseCaseTrackings.Append(new TestStatStore.UseCaseTracking
                    {
                        TestId = createTestCaseResponse.TestId.ToString(),
                        ConceptTypeName = managerTestCase.ConceptInterfaceType.FullName,
                        Context = context,
                        RequestTypeName = managerTestCase.Request.GetType().FullName.Contains(".Request`") ? managerTestCase.Request.GetType().GetProperty("Data").GetValue(managerTestCase.Request).GetType().FullName : managerTestCase.Request.GetType().FullName,
                        ResponseTypeName = expectedResponseData.GetType().FullName,
                        Reasons = createTestCaseResponse.ForkReasons.Select(fr => new TestStatStore.UseCaseTracking.ForkReason
                        {
                            Reason = fr.Reason,
                            Operation = new TestStatStore.UseCaseTracking.Operation
                            {
                                TypeName = fr.Operation.TypeName,
                                RequestTypeName = fr.Operation.RequestTypeName,
                                ResponseTypeName = fr.Operation.ResponseTypeName,
                                Method = fr.Operation.Method
                            }
                        }).ToList(),
                        Operations = operations
                    });
                }
                else if (isUtility)
                {
                    // TODO - we need a list of message listener type wildcards; event, observable...
                    // NOTE: this could be logging...
                    var registeredListener = serviceCollection.Where(sc => sc.ServiceType.FullName == managerTestCase.ConceptInterfaceType.FullName).First();

                    var existingMessageListener = testStatStore.MessageListeners.Where(m => m.ManagerTypeName == registeredListener.ImplementationType.FullName).FirstOrDefault();
                    if (existingMessageListener == null) {
                        existingMessageListener = new TestStatStore.MessageListener
                        {
                            ManagerTypeName = registeredListener.ImplementationType.FullName
                        };
                        testStatStore.MessageListeners = testStatStore.MessageListeners.Append(existingMessageListener);
                        existingMessageListener.RegisteredListeners = existingMessageListener.RegisteredListeners.Append(managerTestCase.ConceptInterfaceType.GenericTypeArguments[0].FullName);
                    }
                }

                return testCaseData;
            });

            foreach (var testCase in createTestCase(testCaseToBuild.Operations, new List<PreregisteredDependency>(), 0, testCaseToBuild, new List<CreateTestCaseResponse.ForkReason>(), testStats, genericTestExpectationComparerType, explicitTestExpectationComparerTypes))
            {
                if (testCase.ExpectedResponse == null)
                {
                    continue;
                }

                var serviceCollection = new ServiceCollection();
                testCaseToBuild.ServiceRegistration?.Invoke(serviceCollection);

                foreach (var svc in testCase.PreregisteredDependencies)
                {
                    serviceCollection.AddScoped(svc.FacetType, sp => svc.Proxy);
                }
                yield return createTestCaseData(testCaseToBuild, serviceCollection, testCase);
            }
        }

        private static IEnumerable<TestStatStore.UseCaseTracking.Operation> getOperationsForTestCase(Guid testId, IEnumerable<UnitTestCase.Operation> operations, TestRunnerConfiguration config, IEnumerable<TestStatStore.UseCaseTracking.Operation> operationChain = null)
        {
            var testCaseOperationChain = operationChain != null ? operationChain.ToList() : new List<TestStatStore.UseCaseTracking.Operation>();

            var unwrapRequest = new Func<object, string>(obj =>
            {
                var isMockExpectedRequestWrapped = config.RequestWrapper != null
                  && obj.GetType().IsGenericType
                  && obj.GetType().GetGenericTypeDefinition() == config.RequestWrapper.OpenWrapperType;

                //    var isMockExpectedResponseWrapped = operation.Res.Item is not null 
                //     && config.ResponseWrapper != null
                //     && mockResponse.Item.GetType().IsGenericType
                //     && mockResponse.Item.GetType().GetGenericTypeDefinition() == config.ResponseWrapper.OpenWrapperType;


                var requestTypeName = isMockExpectedRequestWrapped
                    ? obj.GetType().GetProperty(config.RequestWrapper.DtoPropertyName).GetValue(obj).GetType().FullName
                    : obj.GetType().FullName;
                return requestTypeName;
                //        ResponseTypeName = mockResponse.Item is not null
                //            ? isMockExpectedResponseWrapped
                //                ? mockResponse.Item.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(mockResponse.Item) is not null 
                //                    ? mockResponse.Item.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(mockResponse.Item).GetType().FullName 
                //                    : config.AllowNullResponseData ? string.Empty : throw new Exception("Null response data is not allowed.  This is bad practice.  To allow null, set RunnerOptions.AllowNullResponseData to true.")
                //                : mockResponse.Item.GetType().FullName
                //            : string.Empty
            });

            var unwrapResponse = new Func<object, string>(obj =>
            {
                var isMockExpectedResponseWrapped = obj is not null
                 && config.ResponseWrapper != null
                 && obj.GetType().IsGenericType
                 && obj.GetType().GetGenericTypeDefinition() == config.ResponseWrapper.OpenWrapperType;


                var responseTypeName = obj is not null
                    ? isMockExpectedResponseWrapped
                        ? obj.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(obj) is not null
                            ? obj.GetType().GetProperty(config.ResponseWrapper.DtoPropertyName).GetValue(obj).GetType().FullName
                            : config.AllowNullResponseData ? string.Empty : throw new Exception("Null response data is not allowed.  This is bad practice.  To allow null, set RunnerOptions.AllowNullResponseData to true.")
                        : obj.GetType().FullName
                    : string.Empty;
                return responseTypeName;
            });

            Func<List<TestStatStore.UseCaseTracking.Operation>, UnitTestCase.Response.ForkingResponse, Type, string, object, IEnumerable<TestStatStore.UseCaseTracking.Operation>> handleRightForkDelegate = null;
            handleRightForkDelegate = new Func<List<TestStatStore.UseCaseTracking.Operation>, UnitTestCase.Response.ForkingResponse, Type, string, object, IEnumerable<TestStatStore.UseCaseTracking.Operation>>((ops, forkingResponse, facetType, methodName, expectedRequest) =>
            {
                // the right way could have a forkingResponse.  so delegate this work, passing in a newCallChain
                var rightCallChain = ops.ToList();
                switch (forkingResponse.RightFork.Response)
                {
                    case UnitTestCase.Response.MockResponse resp:
                        {
                            rightCallChain.Add(new TestStatStore.UseCaseTracking.Operation
                            {
                                TypeName = facetType.FullName,
                                Method = methodName,
                                RequestTypeName = unwrapRequest(expectedRequest),
                                ResponseTypeName = unwrapResponse(resp.Response)
                            });
                            var chain = getOperationsForTestCase(testId, forkingResponse.RightFork.Operations, config, rightCallChain);
                            return chain;
                        }
                    case UnitTestCase.Response.ForkingResponse resp:
                        {
                            var newCallChain = rightCallChain.ToList();
                            newCallChain.Add(new TestStatStore.UseCaseTracking.Operation
                            {
                                TypeName = facetType.FullName,
                                Method = methodName,
                                RequestTypeName = unwrapRequest(expectedRequest),
                                ResponseTypeName = unwrapResponse(resp.LeftFork.MockResponse)
                            });
                            var chain = getOperationsForTestCase(testId, resp.LeftFork.Operations, config, newCallChain);
                            if (chain != null)
                            {
                                return chain;
                            }
                            return handleRightForkDelegate(rightCallChain, resp, facetType, methodName, expectedRequest);
                        }
                    default:
                        throw new Exception("Should not get here...");
                }
               
            });

            foreach(var operation in operations)
            {
                switch (operation)
                {
                    case UnitTestCase.ExpectedResponseOperation op:
                        if (op.Id == testId)
                        {
                            // matching chain
                            return testCaseOperationChain;
                        }
                        return null;
                    case UnitTestCase.ServiceOperation op:
                        // we could have a forking response, which we need to convert it to TestStatStore.UseCaseTracking.Operation as we go through each fork
                        switch (op.Response)
                        {
                            case UnitTestCase.Response.MockResponse resp:
                                testCaseOperationChain.Add(new TestStatStore.UseCaseTracking.Operation
                                {
                                    TypeName = op.FacetType.FullName,
                                    Method = op.MethodName,
                                    RequestTypeName = unwrapRequest(op.ExpectedRequest),
                                    ResponseTypeName = unwrapResponse(resp.Response)
                                });
                                break;
                            case UnitTestCase.Response.ForkingResponse resp:
                                // if we are forking, we need to recurse, using the current testCaseOperationChain as a starting point
                                //var asdf = resp.LeftFork.
                                var newCallChain = testCaseOperationChain.ToList();
                                newCallChain.Add(new TestStatStore.UseCaseTracking.Operation
                                {
                                    TypeName = op.FacetType.FullName,
                                    Method = op.MethodName,
                                    RequestTypeName = unwrapRequest(op.ExpectedRequest),
                                    ResponseTypeName = unwrapResponse(resp.LeftFork.MockResponse)
                                });
                                var chain = getOperationsForTestCase(testId, resp.LeftFork.Operations, config, newCallChain);
                                if (chain != null)
                                {
                                    return chain;
                                }

                                var rightForkChain = handleRightForkDelegate(testCaseOperationChain.ToList(), resp, op.FacetType, op.MethodName, op.ExpectedRequest);
                                if (rightForkChain != null)
                                {
                                    return rightForkChain;
                                }
                                break;
                            default:
                                // response is null?
                                // meaning utility.. we still want to show these
                                testCaseOperationChain.Add(new TestStatStore.UseCaseTracking.Operation
                                {
                                    TypeName = op.FacetType.FullName,
                                    Method = op.MethodName,
                                    RequestTypeName = unwrapRequest(op.ExpectedRequest),
                                    ResponseTypeName = string.Empty
                                });
                                break;
                        }

                        break;
                }
            }

            throw new Exception("should we get here?");
        }

        public class TestStats
        {
            /// <summary>
            /// The number of times any service was invoked during a test.
            /// </summary>
            public int InvocationCount { get; set; }

            public TestStats Clone()
            {
                var testStats = new TestStats();
                testStats.InvocationCount = InvocationCount;
                return testStats;
            }
        }

        public static IEnumerable<TestCaseData> TestCaseData()
        {
            var testStatStore = new TestStatStore();
            var integrationTestStats = new IntegrationTestStats();

            var discoveredInterfaceDtosKvp = new Dictionary<string, IEnumerable<Type>>();

            var testCaseTypes = getTestCaseTypes();
            
            var comparerTypes = getComparerTypes();
            Type? genericTestModelComparerType = comparerTypes.Item1; //ITestModelComparer
            var explicitTestModelComparerTypess = comparerTypes.Item2; //ITestModelComparer<T>

            Action writeSystemResults = () =>
            {
                var outputResultsFilePath = InternalTestRunnerConfiguration.SystemResultsOutputPath; // Environment.GetEnvironmentVariable("CoarseSoftwareSystemResults");

                if (!string.IsNullOrEmpty(outputResultsFilePath))
                {
                    var config = Helpers.GetTestRunnerConfiguration();
                    
                    var systemResults = new SystemResults
                    {
                        // TODO - check stats for generic client
                        // when writing to testStatStore, we need to check for message and add the item to a new model to represent that
                        UseCaseTrackings = testStatStore.UseCaseTrackings,
                        MessageListeners = testStatStore.MessageListeners,
                        IntegrationStats = integrationTestStats.IntegrationStats,
                        ClientTestStats = testStatStore.ClientTestStats,
                        Configuration = new SystemResults.Config
                        {
                            Wildcards = config.Wildcard 
                        }
                    };

                    string textPath = Path.Combine(outputResultsFilePath, "CoarseSoftwareSystemResults.json");

                    string json =
                        JsonSerializer.Serialize(systemResults, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }); //, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

                    File.WriteAllText(textPath, $"{json}");
                }
            };

            foreach (var testCaseType in testCaseTypes)
            {
                var testCaseEnumerable = Activator.CreateInstance(testCaseType) as IEnumerable<UnitTestCase>;
                if (testCaseEnumerable == null)
                {
                    throw new Exception($"Could not create instance for type {testCaseType.FullName}");
                }

                foreach (var item in testCaseEnumerable)
                {
                    if (!discoveredInterfaceDtosKvp.ContainsKey(item.ConceptInterfaceType.FullName))
                    {
                        // get all request and response using the base classes by checking all classes (recursively) for matching base
                        // @TODO

                        //testStatStore = discoveredInterfaceDtosKvp[item.ConceptInterfaceType.FullName];
                    }

                    foreach (var testCase in buildTestCaseData(item, new TestStats(), testStatStore, genericTestModelComparerType, explicitTestModelComparerTypess))
                    {
                        yield return testCase;
                    }
                }
            }

            // client tests
            var clientTestCases = getClientTestCaseTypes();
            foreach (var testCaseType in clientTestCases)
            {
                var testCaseEnumerable = Activator.CreateInstance(testCaseType) as IEnumerable<ClientTestCase>;
                if (testCaseEnumerable == null)
                {
                    throw new Exception($"Could not create instance for type {testCaseType.FullName}");
                }
                foreach (var item in testCaseEnumerable)
                {
                    var clientTestStat = new TestStatStore.ClientTestStat();
                    clientTestStat.Id = item.Id;
                    clientTestStat.ClientName = item.Client;
                    clientTestStat.Description = item.Description;
                    testStatStore.ClientTestStats = testStatStore.ClientTestStats.Append(clientTestStat);
                    yield return buildClientTestCaseData(item, clientTestStat, genericTestModelComparerType, explicitTestModelComparerTypess);
                }
            }

            // generic client tests
            var genericClientTestCases = getGenericClientTestCases();
            foreach (var testCaseType in genericClientTestCases)
            {
                var testCaseEnumerable = Activator.CreateInstance(testCaseType) as IEnumerable<GenericClientTestCase>;
                if (testCaseEnumerable == null)
                {
                    throw new Exception($"Could not create instance for type {testCaseType.FullName}");
                }
                foreach (var item in testCaseEnumerable)
                {
                    var serviceStat = new TestStatStore.ServiceStat
                    {
                        TypeName = item.Client
                    };
                    testStatStore.ServiceStats = testStatStore.ServiceStats.Append(serviceStat);
                    yield return GenericClientTestCaseProcessor.Build(item, serviceStat, genericTestModelComparerType, explicitTestModelComparerTypess);
                }
            }

            var integrationTestCaseTypes = getIntegrationTestCaseTypes();
            var counter = 0;
            foreach (var testCaseType in integrationTestCaseTypes)
            {
                var testCaseEnumerable = Activator.CreateInstance(testCaseType) as IEnumerable<IntegrationTestCase>;
                if (testCaseEnumerable == null)
                {
                    throw new Exception($"Could not create instance for type {testCaseType.FullName}");
                }

                counter++;
                var isLast = integrationTestCaseTypes.Count() == counter;
                var internalCounter = 0;
                foreach (var item in testCaseEnumerable)
                {
                    if (!discoveredInterfaceDtosKvp.ContainsKey(item.Service.FacetType.FullName))
                    {
                        // get all request and response using the base classes by checking all classes (recursively) for matching base
                        // @TODO

                        //testStatStore = discoveredInterfaceDtosKvp[item.ConceptInterfaceType.FullName];
                    }
                    internalCounter++;
                    if (isLast && testCaseEnumerable.Count() == internalCounter)
                    {
                        // we have the last test... so pass in a delegate that will be ran to write the output
                        yield return buildIntegrationTestCaseData(item, integrationTestStats, genericTestModelComparerType, explicitTestModelComparerTypess, writeSystemResults);
                    }
                    else
                    {
                        yield return buildIntegrationTestCaseData(item, integrationTestStats, genericTestModelComparerType, explicitTestModelComparerTypess);
                    }
                }
            }
            // @TODO - all tests have run.  Now we can compare the dtos.

            if (!integrationTestCaseTypes.Any())
            {
                writeSystemResults.Invoke();
            }

            
        }

        private static IEnumerable<Type> getTestCaseTypes()
        {
            return getTestCaseTypes<UnitTestCase>();
            //var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //foreach (var assembly in assemblies)
            //{
            //    if (assembly.FullName.Contains("System") || assembly.FullName.Contains("Microsoft"))
            //    {
            //        continue;
            //    }
            //    foreach (var type in assembly.GetTypes())
            //    {
            //        if (type.FullName.Contains("System") || type.FullName.Contains("Microsoft"))
            //        {
            //            continue;
            //        }
            //        var testCaseInterfaces = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).Where(i => i.GetGenericArguments()[0] == typeof(TestCase)).ToList();
            //        if (testCaseInterfaces.Any())
            //        {
            //            yield return type;
            //            continue;
            //        }
            //    }
            //}
        }

        private static IEnumerable<Type> getIntegrationTestCaseTypes()
        {
            //IntegrationTestCase
            return getTestCaseTypes<IntegrationTestCase>();
            //var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //foreach (var assembly in assemblies)
            //{
            //    if (assembly.FullName.Contains("System") || assembly.FullName.Contains("Microsoft"))
            //    {
            //        continue;
            //    }
            //    foreach (var type in assembly.GetTypes())
            //    {
            //        if (type.FullName.Contains("System") || type.FullName.Contains("Microsoft"))
            //        {
            //            continue;
            //        }
            //        var testCaseInterfaces = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).Where(i => i.GetGenericArguments()[0] == typeof(IntegrationTestCase)).ToList();
            //        if (testCaseInterfaces.Any())
            //        {
            //            yield return type;
            //            continue;
            //        }
            //    }
            //}
        }

        private static IEnumerable<Type> getClientTestCaseTypes()
        {
            return getTestCaseTypes<ClientTestCase>();
        }

        private static IEnumerable<Type> getGenericClientTestCases()
        {
            return getTestCaseTypes<GenericClientTestCase>();
        }

        private static IEnumerable<Type> getTestCaseTypes<T>()
        {
            //IntegrationTestCase
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.Contains("System") || assembly.FullName.Contains("Microsoft"))
                {
                    continue;
                }
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName.Contains("System") || type.FullName.Contains("Microsoft"))
                    {
                        continue;
                    }
                    var testCaseInterfaces = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).Where(i => i.GetGenericArguments()[0] == typeof(T)).ToList();
                    if (testCaseInterfaces.Any())
                    {
                        yield return type;
                        continue;
                    }
                }
            }
        }

        private static Tuple<Type, IEnumerable<Type>> getComparerTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type? genericTestModelComparerType = null; //ITestModelComparer
            var explicitTestModelComparerTypess = new List<Type>(); //ITestModelComparer<T>
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.Contains("System") || assembly.FullName.Contains("Microsoft"))
                {
                    continue;
                }
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName.Contains("System") || type.FullName.Contains("Microsoft"))
                    {
                        continue;
                    }

                    if (genericTestModelComparerType == null)
                    {
                        //look for ITestModelComparer
                        if (type.GetInterfaces().Where(i => i == typeof(ITestExpectationComparer)).Any())
                        {
                            genericTestModelComparerType = type;
                            continue;
                        }
                    }
                    //look for ITestModelComparer<T>
                    if (type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITestExpectationComparer<>)).Any())
                    {
                        explicitTestModelComparerTypess.Add(type);
                    }
                }
            }

            if (genericTestModelComparerType == null)
            {
                // no ITestModelComparer implementation found so we default to the basic
                genericTestModelComparerType = typeof(CoarseSoftware.Testing.Framework.Core.Comparer.BasicTestExpectationComparer);
            }

            return new Tuple<Type, IEnumerable<Type>>(genericTestModelComparerType, explicitTestModelComparerTypess);
        }

        public class SystemResults
        {
            public SystemResults()
            {
                this.UseCaseTrackings = new List<TestStatStore.UseCaseTracking>();
                this.MessageListeners = new List<TestStatStore.MessageListener>();
                this.IntegrationStats = new List<IntegrationTestStats.IntegrationStat>();
                this.Configuration = new Config();
            }
            public IEnumerable<TestStatStore.UseCaseTracking> UseCaseTrackings { get; set; }

            public IEnumerable<TestStatStore.MessageListener> MessageListeners { get; set; }
            public IEnumerable<IntegrationTestStats.IntegrationStat> IntegrationStats { get; set; }
            public IEnumerable<TestStatStore.ClientTestStat> ClientTestStats { get; set; }
            public Config Configuration { get; set; }

            public class Config
            {
                public TestRunnerConfiguration.ServiceTypeWildcard Wildcards { get; set; }
            }

        }

        public class VoidResponse { }
    }
}

namespace CoarseSoftware.Testing.Framework.Core.TestCaseProcessor
{
    using CoarseSoftware.Testing.Framework.Core.Proxy;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    public static class GenericClientTestCaseProcessor
    {
        public static TestCaseData Build(GenericClientTestCase genericClientTestCase, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes)
        {
#if NET8_0
            if (genericClientTestCase.EntryPoint.GetType().GetGenericTypeDefinition().Equals(typeof(GenericClientTestCase.WebApplicationEntryPointWrapper<>)))
            {
                var testName = $"{genericClientTestCase.Client} - {genericClientTestCase.Id} | {genericClientTestCase.Description}";

                var testCaseDataRequest = new TestCasesRunner.GenericClientTestCaseDataRequest
                {
                    TestName = testName,
                    EntryPoint = () =>
                    {
                        // most of this is in the runner.
                        Type genericClientTestCaseType = genericClientTestCase.EntryPoint.GetType();
                        var convert_method = genericClientTestCaseType.GetMethod("InvokeEntryPoint");
                        var config = Helpers.GetTestRunnerConfiguration();

                        var result = convert_method.Invoke(genericClientTestCase.EntryPoint, new object[]
                        {
                            genericTestExpectationComparerType,
                            explicitTestExpectationComparerTypes,
                            genericClientTestCase.Service,
                            config
                        });
                        return result;
                    }
                };

                var testCaseData = new TestCaseData(testCaseDataRequest, new TestStatStore());

                testCaseData.SetCategory($"{testName}");
                testCaseData.SetName(testCaseDataRequest.TestName);
                testCaseData.Returns(true);

                return testCaseData;                
            }
#endif
            switch (genericClientTestCase.EntryPoint)
            {
                case GenericClientTestCase.EmptyEntryPointWrapper entry:
                    {
                        break;
                    }
                case GenericClientTestCase.ServiceCollectionEntryPointWrapper entry:
                    {
                        break;
                    }
                case GenericClientTestCase.ServiceProviderEntryPointWrapper entry:
                    {
                        break;
                    }
            }
            throw new Exception("Should not get here...");
        }

        public static async Task<bool> Run(TestCasesRunner.GenericClientTestCaseDataRequest testCase)
        {
            var entryPointResponse = testCase.EntryPoint.Invoke();
            // run comparison
            return true;

            //var comparerTypes = getComparerTypes();
            //Type? genericTestModelComparerType = comparerTypes.Item1; //ITestModelComparer
            //var explicitTestModelComparerTypess = comparerTypes.Item2; //ITestModelComparer<T>

            //var unwrappedTaskResponse = 
            //var responseType = entryPointResponse.GetType();
            //var isTask = responseType.Equals(typeof(Task)) || (responseType.IsGenericType && responseType.GetGenericTypeDefinition().Equals(typeof(Task<>)));
            //object response = null;
            ////firstArgType.GetGenericTypeDefinition() == configuration.RequestWrapper.OpenWrapperType;
            //if (isTask)
            //{
            //    ((Task)entryPointResponse).Wait();

            //    var resultProperty = responseType.GetProperty("Result");
            //    response = resultProperty.GetValue(entryPointResponse);
            //}
            //else
            //{
            //    response = entryPointResponse;
            //}

            //var expectedResponseType = testCase.ExpectedResponse.GetType();
            //isTask = expectedResponseType.Equals(typeof(Task)) || (expectedResponseType.IsGenericType && expectedResponseType.GetGenericTypeDefinition().Equals(typeof(Task<>)));
            //object expectedResponse = null;
            ////firstArgType.GetGenericTypeDefinition() == configuration.RequestWrapper.OpenWrapperType;
            //if (isTask)
            //{
            //    ((Task)testCase.ExpectedResponse).Wait();

            //    var resultProperty = expectedResponseType.GetProperty("Result");
            //    expectedResponse = resultProperty.GetValue(testCase.ExpectedResponse);
            //}
            //else
            //{
            //    expectedResponse = testCase.ExpectedResponse;
            //}

            //// compare
            //Helpers.CompareExpectedToActual(expectedResponse, response, testCase.IngoredExpectedResponsePropertyNames, explicitTestModelComparerTypess, genericTestModelComparerType);

            //return true;
        }
    }
}

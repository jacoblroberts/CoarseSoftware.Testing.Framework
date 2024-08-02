namespace CoarseSoftware.Testing.Framework.Core
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A unit test case
    /// </summary>
    public class UnitTestCase
    {
        public UnitTestCase(string description)
        {
            this.Description = description;
        }
        /// <summary>
        /// this is used to describe the test
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Concepts; manager, engine, access, 
        /// </summary>
        public Type ConceptInterfaceType { get; set; }

        /// <summary>
        /// Method name like; FlowAsync
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Used to register services that are not mocked or supplied from Hostings.  ie; unit of work...
        /// </summary>
        public Action<IServiceCollection> ServiceRegistration { get; set; }

        /// <summary>
        /// The request object
        /// </summary>
        public object Request { get; set; }

        /// <summary>
        /// Ordered service invocations; utilities and components
        /// </summary>
        public IEnumerable<Operation> Operations { get; set; }

        /// <summary>
        /// The expected service operations, in order, that will be invoked.
        /// </summary>
        public class Operation { }

        public class ServiceOperation : Operation
        {
            /// <summary>
            /// The expected service that is invoked.
            /// </summary>
            public Type FacetType { get; set; }

            /// <summary>
            /// The FacetType.Method that is being invoked.
            /// </summary>
            public string MethodName { get; set; }

            /// <summary>
            /// The expected request
            /// </summary>
            public object ExpectedRequest { get; set; }
            public IEnumerable<string> IngoredExpectedRequestPropertyNames { get; set; }

            /// <summary>
            /// The response expectations.
            /// </summary>
            public Response Response { get; set; }
        }

        /// <summary>
        /// Response base class
        /// </summary>
        public class Response
        {
            /// <summary>
            /// When we need a simple response and do not care about downstream conditions.
            /// </summary>
            public class MockResponse : Response
            {
                public object Response { get; set; }
            }

            /// <summary>
            /// A response that allows splitting conditional logic.  ie; testing valid and invalid process flow.
            /// Each ForkingResponse creates an additional test.
            /// The RightFork can have a ForkingResponse, allowing for if-then-else.  Produces 3 tests.   
            /// </summary>
            public class ForkingResponse : Response
            {
                public LeftForkResponse LeftFork { get; set; }
                public RightForkResponse RightFork { get; set; }

                public class LeftForkResponse
                {
                    public string Reason { get; set; }
                    public object MockResponse { get; set; }
                    public IEnumerable<Operation> Operations { get; set; }
                }

                public class RightForkResponse
                {
                    public string Reason { get; set; }
                    public Response Response { get; set; }
                    public IEnumerable<Operation> Operations { get; set; }
                }
            }
        }

        /// <summary>
        /// This completes a test generation.
        /// </summary>
        public class ExpectedResponseOperation : Operation
        {
            /// <summary>
            /// Using an ID to make it easier to find the test case.
            /// </summary>
            public Guid Id { get; set; }
            public object ExpectedResponse { get; set; }
            public IEnumerable<string> IngoredExpectedResponsePropertyNames { get; set; }
        }
    }


    /// <summary>
    /// Integration test case
    /// </summary>
    public class IntegrationTestCase
    {
        public IntegrationTestCase(string description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Unique/constant test Id.
        /// Note: if this is not constant (ie; Guid.Parse(guidString)), it will cause test discovery to find new tests every time.  You will know this is happening if your successful tests flip to not ran.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Client name
        /// </summary>
        public string Client { get; set; }

        /// <summary>
        /// Describes the test.
        /// Note: Could be used to provide some context clues as to which UI page this request should produce.
        /// </summary>
        public string Description {  get; private set; }

        /// <summary>
        /// Used to register services.
        /// Note: if you want to overwrite a facet (ie; mock resource), register it last.  This way, if anything registers an instance, the last registration will win.
        /// </summary>
        public Action<IServiceCollection> ServiceRegistration { get; set; }

        /// <summary>
        /// This is used to setup and run the client entry points.
        /// This can be used to test full middleware or invoking the controller directly.  Possibly instantiating your MVVM and invoking a Command.
        /// Generics - response is the expected response.  Possibly null.
        /// </summary>
        //public Func<IServiceProvider, object> EntryPoint { get; set; }

        /// <summary>
        /// This is the expected response from the EntryPoint invocation
        /// </summary>
        //public object ExpectedResponse {  set; get; }
        //public IEnumerable<string> IngoredExpectedResponsePropertyNames { get; set; }

        /// <summary>
        /// Expectations of the microservice that will be invoked.
        /// </summary>
        public Microservice Service { get; set; }


        public class Microservice
        {
            /// <summary>
            /// The microservice we intend to invoke.  This will be any Manager service.
            /// </summary>
            public Type FacetType { get; set; }

            /// <summary>
            /// The FacetType method to invoke
            /// </summary>
            public string MethodName { get; set; }

            /// <summary>
            /// Expected request.
            /// </summary>
            public object Request {  get; set; }

            /// <summary>
            /// Expected response.
            /// </summary>
            public object ExpectedResponse { get; set; }

            public IEnumerable<string> IngoredResponsePropertyNames { get; set; }
        }
        /*
c) client integration tests
    may need to open up an array of args
      this way the request can be a http request mock with json body and endpoint.  ie; testing the full middleware into controller invocation
    alternatevly, can simply set the http context (ie; token, user, roles etc...) and then invoke the controller directly.
    either way, we will get a request/response at the manager invocation.  (proxy wrapper around manager so the model expectation assertions can be made)
      this will allow us to map the request and response the get a client for the call chains and sequence diagrams.  Also in static architecutre


client integration tests
  TestCase will be a base class to UnitTestCase and IntegrationTestCase.  The test case builder will iterate both.  the test will switch on type (so we wont have multiple tests that show but do not run).
         we only need the TestCaseDataRequest as a base... possibly need to make the current take only one object that has the current args.
  IntegrationTestCase         
         */
    }
}

namespace CoarseSoftware.Testing.Framework.Core
{
    using Microsoft.Extensions.DependencyInjection;
    public class TestCase
    {
        public TestCase(string description)
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

        public class Operation { }

        public class ServiceOperation : Operation
        {
            /// <summary>
            /// We need type here instead of string due to utility naming not being inferrable
            /// </summary>
            public Type FacetType { get; set; }
            public string MethodName { get; set; }

            public object ExpectedRequest { get; set; }
            public IEnumerable<string> IngoredExpectedRequestPropertyNames { get; set; }
            public Response Response { get; set; }
        }
        public class Response
        {
            public class MockResponse : Response
            {
                public object Response { get; set; }
            }
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
}

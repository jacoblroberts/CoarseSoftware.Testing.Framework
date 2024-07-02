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
        public string Method { get; set; } // how do we make selecting the method easy?

        /// <summary>
        /// This is for any extra arguments that a Hosting.Register takes.  
        /// The IServiceCollection is implied and set as the first argument (forcing best practices).  The second argument of hosting is sometimes a delegate for configuration.  That delegate and anything else needs to be added here, in order. 
        /// </summary>
        public IEnumerable<object> OptionalHostingArgs { get; set; } = new List<object>();

        /// <summary>
        /// Used to register services that are not mocked or supplied from Hostings.  ie; unit of work...
        /// </summary>
        public Action<IServiceCollection> ServiceRegistration { get; set; }

        /// <summary>
        /// The request object
        /// </summary>
        public object Request { get; set; }

        // ExpectedResponse could change based on forked operation
        //  a forked operation can change the 'final response' or simply invoke operations
        //  when a 'final response' is seen, the test is created and the rest of the items are traversed.

        /// <summary>
        /// Ordered service invocations; utilities and components
        /// </summary>
        public IEnumerable<Operation> Operations { get; set; }

        public class Operation { }

        public class SingleOperation : Operation
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
                public ForkMockResponse LeftFork { get; set; }
                public ForkMockResponse RightFork { get; set; }

                public class ForkMockResponse
                {
                    public string Reason { get; set; }
                    public object Response { get; set; }
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

        //public class ForkOperation : Operation
        //{
        //    /// <summary>
        //    /// this is used when there is a condition that doesn't return but perhaps does something like send a notification.
        //    /// example;  if file processesd, notify the member, do more stuff after the conditional scope....
        //    /// </summary>
        //    public string Reason { get; set; }
        //    public IEnumerable<Operation> Operations { get; set; }
        //}


    }
}

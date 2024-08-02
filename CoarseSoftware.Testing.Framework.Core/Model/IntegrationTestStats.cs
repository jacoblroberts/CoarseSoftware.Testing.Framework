namespace CoarseSoftware.Testing.Framework.Core
{
    public class IntegrationTestStats
    {
        // we create this once and when we create a service proxy, we pass Invocation in.  
        public IntegrationTestStats()
        {
            this.IntegrationStats = new List<IntegrationStat>();
        }
        /// <summary>
        /// Each manager invocation will create an entry.  That entry is passed down so child entries can be created.
        /// </summary>
        public IEnumerable<IntegrationStat> IntegrationStats { get; set; }

        public class IntegrationStat
        {
            public string Client { get; set; }
            public string Description { get; set; }
            public Invocation MicroserviceInvocation { get; set; }
}

        public class Invocation
        {
            public Invocation()
            {
                this.ChildInvocations = new List<Invocation>();
            }
            public string ServiceType { get; set; }
            public string Method { get; set; }
            public string RequestType { get; set; }
            public string ResponseType { get; set; }
            public IEnumerable<Invocation> ChildInvocations { get; set; }
        }
    }
}

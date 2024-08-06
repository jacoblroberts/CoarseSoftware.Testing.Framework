namespace CoarseSoftware.Testing.Framework.Examples.Test.NUnit.UnitTest
{
    using CoarseSoftware.BusinessSystem.iFX;

    using JobResourceFacet = CoarseSoftware.BusinessSystem.Resource.Job.Interface;

    using JobAccessFacet = CoarseSoftware.BusinessSystem.Component.Access.Job.Interface;

    using global::System.Collections;
    using CoarseSoftware.Testing.Framework.Core;
    using Microsoft.Extensions.DependencyInjection;

    public class AccessTestCases : IEnumerable<UnitTestCase>
    {
        public IEnumerator<UnitTestCase> GetEnumerator()
        {
            yield return new UnitTestCase("Job Access")
            {
                ServiceRegistration = serviceCollection =>
                {
                    serviceCollection.AddScoped<JobAccessFacet.IJobAccess, CoarseSoftware.BusinessSystem.Component.Access.Job.Service.JobAccess>();
                },
                ConceptInterfaceType = typeof(JobAccessFacet.IJobAccess),
                Method = "FilterAsync",
                Request = new Request<JobAccessFacet.RequestBase>
                {
                    Data = new JobAccessFacet.RequestBase
                    { }
                },
                Operations = new List<UnitTestCase.Operation>
                {
                    new UnitTestCase.ServiceOperation
                    {
                        FacetType = typeof(JobResourceFacet.IJobResource),
                        MethodName = "ListAsync",
                        ExpectedRequest = new Request<JobResourceFacet.RequestBase>
                        {
                            Data = new JobResourceFacet.RequestBase
                            { }
                        },
                        Response = new UnitTestCase.Response.MockResponse
                        {
                            Response = new Response<JobResourceFacet.ResponseBase>
                            {
                                Data = new JobResourceFacet.ResponseBase
                                { }
                            }
                        }
                    },
                    new UnitTestCase.ExpectedResponseOperation
                    {
                        Id = Guid.Parse("703cd803-1562-49ae-bd23-a80f635bf2a8"),
                        ExpectedResponse = new Response<JobAccessFacet.ResponseBase>
                        {
                            Data = new JobAccessFacet.ResponseBase
                            { }
                        }
                    }
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

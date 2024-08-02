namespace CoarseSoftware.Testing.Framework.Examples.Test.NUnit.UnitTest
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;

    using JobAccessFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Access.Job.Interface;
    
    using RegulatingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating.Interface;
    using TransformingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Transforming.Interface;
    using ValidatingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Validating.Interface;

    using global::System.Collections;
    using CoarseSoftware.Testing.Framework.Core;
    using Microsoft.Extensions.DependencyInjection;

    public class EngineTestCases : IEnumerable<UnitTestCase>
    {
        public IEnumerator<UnitTestCase> GetEnumerator()
        {
            yield return new UnitTestCase("Regulating engine")
            {
                ServiceRegistration = serviceCollection =>
                {
                    serviceCollection.AddScoped<RegulatingEngineFacet.IRegulatingEngine, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating.Service.RegulatingEngine>();
                },
                ConceptInterfaceType = typeof(RegulatingEngineFacet.IRegulatingEngine),
                Method = "ApplyAsync",
                Request = new Request<RegulatingEngineFacet.RequestBase>
                {
                    Data = new RegulatingEngineFacet.RequestBase
                    { }
                },
                Operations = new List<UnitTestCase.Operation>
                {
                    new UnitTestCase.ServiceOperation
                    {
                        FacetType = typeof(JobAccessFacet.IJobAccess),
                        MethodName = "FilterAsync",
                        ExpectedRequest = new Request<JobAccessFacet.RequestBase>
                        {
                            Data = new JobAccessFacet.RequestBase
                            { }
                        },
                        Response = new UnitTestCase.Response.MockResponse
                        {
                            Response = new Response<JobAccessFacet.ResponseBase>
                            {
                                Data = new JobAccessFacet.ResponseBase
                                { }
                            }
                        }
                    },
                    new UnitTestCase.ExpectedResponseOperation
                    {
                        Id = Guid.Parse("9aa3ee67-130d-43b2-8bf6-fe4893859e03"),
                        ExpectedResponse = new Response<RegulatingEngineFacet.ResponseBase>
                        {
                            Data = new RegulatingEngineFacet.ResponseBase
                            { }
                        }
                    }
                }
            };

            yield return new UnitTestCase("Transforming engine")
            {
                ServiceRegistration = serviceCollection =>
                {
                    serviceCollection.AddScoped<TransformingEngineFacet.ITransformingEngine, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Transforming.Service.TransformingEngine>();
                },
                ConceptInterfaceType = typeof(TransformingEngineFacet.ITransformingEngine),
                Method = "TransformAsync",
                Request = new Request<TransformingEngineFacet.RequestBase>
                {
                    Data = new TransformingEngineFacet.RequestBase
                    { }
                },
                Operations = new List<UnitTestCase.Operation>
                {
                    new UnitTestCase.ServiceOperation
                    {
                        FacetType = typeof(JobAccessFacet.IJobAccess),
                        MethodName = "FilterAsync",
                        ExpectedRequest = new Request<JobAccessFacet.RequestBase>
                        {
                            Data = new JobAccessFacet.RequestBase
                            { }
                        },
                        Response = new UnitTestCase.Response.MockResponse
                        {
                            Response = new Response<JobAccessFacet.ResponseBase>
                            {
                                Data = new JobAccessFacet.ResponseBase
                                { }
                            }
                        }
                    },
                    new UnitTestCase.ExpectedResponseOperation
                    {
                        Id = Guid.Parse("3c7bd571-dd33-467a-b023-be4d1781cc32"),
                        ExpectedResponse = new Response<TransformingEngineFacet.ResponseBase>
                        {
                            Data = new TransformingEngineFacet.ResponseBase
                            { }
                        }
                    }
                }
            };

            yield return new UnitTestCase("Validating engine")
            {
                ServiceRegistration = serviceCollection =>
                {
                    serviceCollection.AddScoped<ValidatingEngineFacet.IValidatingEngine, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Validating.Service.ValidatingEngine>();
                },
                ConceptInterfaceType = typeof(ValidatingEngineFacet.IValidatingEngine),
                Method = "ValidateAsync",
                Request = new Request<ValidatingEngineFacet.RequestBase>
                {
                    Data = new ValidatingEngineFacet.RequestBase
                    { }
                },
                Operations = new List<UnitTestCase.Operation>
                {
                    new UnitTestCase.ExpectedResponseOperation
                    {
                        Id = Guid.Parse("adf771f9-5c63-4c51-82bc-65609809d619"),
                        ExpectedResponse = new Response<ValidatingEngineFacet.ResponseBase>
                        {
                            Data = new ValidatingEngineFacet.ResponseBase
                            {
                                IsValid = true
                            }
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

namespace CoarseSoftware.Testing.Framework.Examples.Test.NUnit.UnitTest
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;
    
    using RegulatingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating;
    using TransformingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Transforming;
    using ValidatingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Validating;

    using DashboardManagerFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Dashboard;
    using AdministrationManagerFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration;

    using global::NUnit.Framework;
    using global::System.Collections;
    using CoarseSoftware.Testing.Framework.Core;
    using Microsoft.Extensions.DependencyInjection;

    [TestFixture]
    public class Testing : TestCasesRunner
    {

    }

    public class TestCases : IEnumerable<TestCase>
    {
        public IEnumerator<TestCase> GetEnumerator()
        {
            yield return new TestCase("Dashboard request has expected response")
            {
                ServiceRegistration = serviceCollection =>
                {
                    serviceCollection.AddScoped<DashboardManagerFacet.IDashboardManager, DashboardManagerFacet.DashboardManager>();
                },
                ConceptInterfaceType = typeof(DashboardManagerFacet.IDashboardManager),
                Method = "FlowAsync",
                Request = new Request<DashboardManagerFacet.OnStepCompleteBase>
                {
                    Data = new DashboardManagerFacet.OnStepCompleteBase
                    { }
                },
                Operations = new List<TestCase.Operation>
                {
                    new TestCase.SingleOperation
                    {
                        FacetType = typeof(RegulatingEngineFacet.IRegulatingEngine),
                        MethodName = "ApplyAsync",
                        ExpectedRequest = new Request<RegulatingEngineFacet.RequestBase>
                        {
                            Data = new RegulatingEngineFacet.RequestBase
                            {
                                Numbers = new List<int>
                                {
                                    1, 2, 3, 6
                                }
                            }
                        },
                        Response = new TestCase.Response.MockResponse
                        {
                            Response = new Response<RegulatingEngineFacet.ResponseBase>
                            {
                                Data = new RegulatingEngineFacet.ResponseBase
                                { }
                            }
                        },
                        IngoredExpectedRequestPropertyNames = new string[] { }
                    },
                    new TestCase.SingleOperation
                    {
                        FacetType = typeof(ValidatingEngineFacet.IValidatingEngine),
                        MethodName = "ValidateAsync",
                        ExpectedRequest = new Request<ValidatingEngineFacet.RequestBase>
                        {
                            Data = new ValidatingEngineFacet.RequestBase
                            { }
                        },
                        Response = new TestCase.Response.ForkingResponse
                        {
                            LeftFork = new TestCase.Response.ForkingResponse.ForkMockResponse
                            {
                                Reason = "Failed Validation",
                                Response = new Response<ValidatingEngineFacet.ResponseBase>
                                {
                                    Data = new ValidatingEngineFacet.ResponseBase
                                    {
                                        IsValid = false
                                    }
                                },
                                Operations = new List<TestCase.Operation>
                                {
                                    new TestCase.SingleOperation
                                    {
                                        FacetType = typeof(TransformingEngineFacet.ITransformingEngine),
                                        MethodName = "TransformAsync",
                                        ExpectedRequest = new Request<TransformingEngineFacet.RequestBase>
                                        {
                                            Data = new TransformingEngineFacet.RequestBase
                                            { }
                                        },
                                        Response = new TestCase.Response.MockResponse
                                        {
                                            Response = new Response<TransformingEngineFacet.ResponseBase>
                                            {
                                                Data = new TransformingEngineFacet.ResponseBase
                                                { }
                                            }
                                        }
                                    },
                                    new TestCase.ExpectedResponseOperation
                                    {
                                        Id = Guid.Parse("d1310015-85f4-4e9a-9e8e-ef07dd35fda5"),
                                        ExpectedResponse = new Response<DashboardManagerFacet.OnStepActivateBase>{
                                            Data = new DashboardManagerFacet.OnStepActivateBase
                                            { }
                                        }
                                    }
                                }
                            },
                            RightFork = new TestCase.Response.ForkingResponse.ForkMockResponse
                            {
                                Reason = "Passed Validation",
                                Response = new Response<ValidatingEngineFacet.ResponseBase>
                                {
                                    Data = new ValidatingEngineFacet.ResponseBase
                                    {
                                        IsValid = true
                                    }
                                },
                                Operations = new List<TestCase.Operation>
                                {
                                    new TestCase.ExpectedResponseOperation
                                    {
                                        Id = Guid.Parse("b6dfb0bd-8535-402c-8ee2-d4558eeabefa"),
                                        ExpectedResponse = new Response<DashboardManagerFacet.OnStepActivateBase> {
                                            Data = new DashboardManagerFacet.DerivedOnStepActivate
                                            {
                                                StaticId = "SomeStaticId"
                                            }
                                        },
                                        IngoredExpectedResponsePropertyNames = new List<string>
                                        {
                                            "RandomId"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            yield return new TestCase("Administration request has expected response")
            {
                ServiceRegistration = (serviceCollection) =>
                {
                    serviceCollection.AddScoped<AdministrationManagerFacet.IAdministrationManager, AdministrationManagerFacet.AdministrationManager>();
                },
                ConceptInterfaceType = typeof(AdministrationManagerFacet.IAdministrationManager),
                Method = "FlowAsync",
                Request = new Request<AdministrationManagerFacet.OnStepCompleteBase>
                {
                    Data = new AdministrationManagerFacet.OnStepCompleteBase
                    { }
                },
                Operations = new List<TestCase.Operation>
                {
                    new TestCase.ExpectedResponseOperation
                    {
                        Id = Guid.Parse("0a8b2a8e-1447-4fa9-8770-fab2d4e8c0cd"),
                        ExpectedResponse = new Response<AdministrationManagerFacet.OnStepActivateBase>
                        {
                            Data = new AdministrationManagerFacet.OnStepActivateBase
                            {
                                SomeBaseClassField = "Hello World"
                            }
                        }
                    }
                }
            };

            // @TODO - bring in message handler example
            //yield return new TestCase("Process that returns void")
            //{
            //    ServiceRegistration = serviceCollection =>
            //    {
            //        Examples.Manager.Administration.Service.Hosting.Register(serviceCollection);
            //    },
            //    ConceptInterfaceType = typeof(Examples.iFX.Common.Message.Event.IMessageEventListener<Examples.iFX.Common.Message.Event.Membership.MembershipMessageEvent>),
            //    Method = "ProcessAsync",
            //    Request = new Examples.iFX.Common.Message.Event.Membership.MembershipMessageEvent
            //    {
            //        SomeField = "message received"
            //    },
            //    Operations = new List<TestCase.Operation>
            //    {
            //        new TestCase.SingleOperation
            //        {
            //            FacetType = typeof(Examples.iFX.Common.Message.Event.IMessageEvent<Examples.iFX.Common.Message.Event.Membership.MembershipMessageEvent>),
            //            MethodName = "PublishAsync",
            //            ExpectedRequest = new Examples.iFX.Common.Message.Event.Membership.MembershipMessageEvent
            //            {
            //                SomeField = "GOTCHA!"
            //            }
            //        },
            //        new TestCase.ExpectedResponseOperation
            //        {
            //            Id = Guid.Parse("1dfec370-8556-4f8b-9ba7-ed08268e7623")
            //        }
            //    }
            //};

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

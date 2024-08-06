namespace CoarseSoftware.Testing.Framework.Examples.Test.NUnit.UnitTest
{
    using CoarseSoftware.BusinessSystem.iFX;
    using CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Logging;

    using JobAccessFacet = CoarseSoftware.BusinessSystem.Component.Access.Job.Interface;

    using RegulatingEngineFacet = CoarseSoftware.BusinessSystem.Component.Engine.Regulating.Interface;
    using TransformingEngineFacet = CoarseSoftware.BusinessSystem.Component.Engine.Transforming.Interface;
    using ValidatingEngineFacet = CoarseSoftware.BusinessSystem.Component.Engine.Validating.Interface;

    using DashboardManagerFacet = CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface;
    using AdministrationManagerFacet = CoarseSoftware.BusinessSystem.Component.Manager.Administration.Interface;

    using global::System.Collections;
    using CoarseSoftware.Testing.Framework.Core;
    using Microsoft.Extensions.DependencyInjection;
    using CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Event.Model;
    using CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Event;

    public class TestCases : IEnumerable<UnitTestCase>
    {
        public IEnumerator<UnitTestCase> GetEnumerator()
        {
            yield return new UnitTestCase("Dashboard request has expected response")
            {
                ServiceRegistration = serviceCollection =>
                {
                    serviceCollection.AddScoped<DashboardManagerFacet.IDashboardManager, CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Service.DashboardManager>();
                },
                ConceptInterfaceType = typeof(DashboardManagerFacet.IDashboardManager),
                Method = "FlowAsync",
                Request = new Request<DashboardManagerFacet.OnStepCompleteBase>
                {
                    Data = new DashboardManagerFacet.OnStepCompleteBase
                    { }
                },
                Operations = new List<UnitTestCase.Operation>
                {
                    new UnitTestCase.ServiceOperation
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
                        //Response = new TestCase.Response.MockResponse
                        //{
                        //    Response = new Response<RegulatingEngineFacet.ResponseBase>
                        //    {
                        //        Data = new RegulatingEngineFacet.ResponseBase
                        //        { }
                        //    }
                        //},
                        Response = new UnitTestCase.Response.ForkingResponse
                        {
                            LeftFork = new UnitTestCase.Response.ForkingResponse.LeftForkResponse
                            {
                                Reason = "number is 1",
                                MockResponse = new Response<RegulatingEngineFacet.ResponseBase>
                                {
                                    Data = new RegulatingEngineFacet.ResponseBase
                                    {
                                        Number = 1
                                    }
                                },
                                Operations = new List<UnitTestCase.Operation>
                                {
                                    new UnitTestCase.ExpectedResponseOperation
                                    {
                                        Id = Guid.Parse("3f1c761e-6892-4271-8c98-d9293a8a91f9"),
                                        ExpectedResponse = new Response<DashboardManagerFacet.OnStepActivateBase> {
                                            Data = new DashboardManagerFacet.DerivedOnStepActivate
                                            {
                                                StaticId = "IdNumber1"
                                            }
                                        },
                                        IngoredExpectedResponsePropertyNames = new List<string>
                                        {
                                            "RandomId"
                                        }
                                    }
                                }
                            },
                            RightFork = new UnitTestCase.Response.ForkingResponse.RightForkResponse
                            {
                                Response = new UnitTestCase.Response.ForkingResponse
                                {
                                    LeftFork = new UnitTestCase.Response.ForkingResponse.LeftForkResponse
                                    {
                                        Reason = "number is 2",
                                        MockResponse = new Response<RegulatingEngineFacet.ResponseBase>
                                        {
                                            Data = new RegulatingEngineFacet.ResponseBase
                                            {
                                                Number = 2
                                            }
                                        },
                                        Operations = new List<UnitTestCase.Operation>
                                        {
                                            new UnitTestCase.ExpectedResponseOperation
                                            {
                                                Id = Guid.Parse("e3a433ef-8cd4-47a2-85bd-e9861d2accf9"),
                                                ExpectedResponse = new Response<DashboardManagerFacet.OnStepActivateBase> {
                                                    Data = new DashboardManagerFacet.DerivedOnStepActivate
                                                    {
                                                        StaticId = "IdNumber2"
                                                    }
                                                },
                                                IngoredExpectedResponsePropertyNames = new List<string>
                                                {
                                                    "RandomId"
                                                }
                                            }
                                        }
                                    },
                                    RightFork = new UnitTestCase.Response.ForkingResponse.RightForkResponse
                                    {
                                        Reason = "number is not 1 or 2",
                                        Response = new UnitTestCase.Response.MockResponse
                                        {
                                            Response = new Response<RegulatingEngineFacet.ResponseBase>
                                            {
                                                Data = new RegulatingEngineFacet.ResponseBase
                                                {
                                                    Number = 0
                                                }
                                            }
                                        },
                                        Operations = new List<UnitTestCase.Operation>
                                        {
                                            new UnitTestCase.ServiceOperation
                                            {
                                                FacetType = typeof(ValidatingEngineFacet.IValidatingEngine),
                                                MethodName = "ValidateAsync",
                                                ExpectedRequest = new Request<ValidatingEngineFacet.RequestBase>
                                                {
                                                    Data = new ValidatingEngineFacet.RequestBase
                                                    { }
                                                },
                                                Response = new UnitTestCase.Response.ForkingResponse
                                                {
                                                    LeftFork = new UnitTestCase.Response.ForkingResponse.LeftForkResponse
                                                    {
                                                        Reason = "Failed Validation",
                                                        MockResponse = new Response<ValidatingEngineFacet.ResponseBase>
                                                        {
                                                            Data = new ValidatingEngineFacet.ResponseBase
                                                            {
                                                                IsValid = false
                                                            }
                                                        },
                                                        Operations = new List<UnitTestCase.Operation>
                                                        {
                                                            new UnitTestCase.ServiceOperation
                                                            {
                                                                FacetType = typeof(TransformingEngineFacet.ITransformingEngine),
                                                                MethodName = "TransformAsync",
                                                                ExpectedRequest = new Request<TransformingEngineFacet.RequestBase>
                                                                {
                                                                    Data = new TransformingEngineFacet.RequestBase
                                                                    { }
                                                                },
                                                                Response = new UnitTestCase.Response.MockResponse
                                                                {
                                                                    Response = new Response<TransformingEngineFacet.ResponseBase>
                                                                    {
                                                                        Data = new TransformingEngineFacet.ResponseBase
                                                                        { }
                                                                    }
                                                                }
                                                            },
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
                                                                Id = Guid.Parse("d1310015-85f4-4e9a-9e8e-ef07dd35fda5"),
                                                                ExpectedResponse = new Response<DashboardManagerFacet.OnStepActivateBase>{
                                                                    Data = new DashboardManagerFacet.OnStepActivateBase
                                                                    { }
                                                                }
                                                            }
                                                        }
                                                    },
                                                    RightFork = new UnitTestCase.Response.ForkingResponse.RightForkResponse
                                                    {
                                                        Reason = "Passed Validation",
                                                        Response = new UnitTestCase.Response.MockResponse
                                                        {
                                                            Response = new Response<ValidatingEngineFacet.ResponseBase>
                                                            {
                                                                Data = new ValidatingEngineFacet.ResponseBase
                                                                {
                                                                    IsValid = true
                                                                }
                                                            }
                                                        },
                                                        Operations = new List<UnitTestCase.Operation>
                                                        {
                                                            new UnitTestCase.ExpectedResponseOperation
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
                                    }
                                }
                            }
                        }
                    }
                }
            };

            yield return new UnitTestCase("Dashboard message event listener: SomeCrossVolatilityMessageEvent")
            {
                ServiceRegistration = (serviceCollection) =>
                {
                    serviceCollection.AddScoped<IMessageEventListener<SomeCrossVolatilityMessageEvent>, CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Service.DashboardManager>();
                },
                ConceptInterfaceType = typeof(IMessageEventListener<SomeCrossVolatilityMessageEvent>),
                Method = "ProcessAsync",
                Request = new SomeCrossVolatilityMessageEvent
                {
                    SomeProperty = "bla bla bla"
                },
                Operations = new List<UnitTestCase.Operation>
                {
                    new UnitTestCase.ExpectedResponseOperation
                    {
                        Id = Guid.Parse("0a807593-422a-4bc5-b288-69a492440e3f")
                    }
                }
            };

            yield return new UnitTestCase("Administration request has expected response")
            {
                ServiceRegistration = (serviceCollection) =>
                {
                    serviceCollection.AddScoped<AdministrationManagerFacet.IAdministrationManager, CoarseSoftware.BusinessSystem.Component.Manager.Administration.Service.AdministrationManager>();
                },
                ConceptInterfaceType = typeof(AdministrationManagerFacet.IAdministrationManager),
                Method = "FlowAsync",
                Request = new Request<AdministrationManagerFacet.OnStepCompleteBase>
                {
                    Data = new AdministrationManagerFacet.OnStepCompleteBase
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
                    new UnitTestCase.ServiceOperation
                    {
                        FacetType = typeof(ILogger),
                        MethodName = "LogInformation",
                        ExpectedRequest = "Logging some info"
                    },
                    new UnitTestCase.ServiceOperation
                    {
                        FacetType = typeof(IMessageEvent<SomeCrossVolatilityMessageEvent>),
                        MethodName = "PublishAsync",
                        ExpectedRequest = new SomeCrossVolatilityMessageEvent
                        {
                            SomeProperty = "a property value"
                        }
                    },
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

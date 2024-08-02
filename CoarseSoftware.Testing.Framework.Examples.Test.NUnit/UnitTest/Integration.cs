namespace CoarseSoftware.Testing.Framework.Examples.Test.NUnit.UnitTest
{
    using global::System.Collections;
    using CoarseSoftware.Testing.Framework.Core;
    using Microsoft.Extensions.DependencyInjection;

    using DashboardManagerFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Dashboard.Interface;
    using AdministrationManagerFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface;

    using RegulatingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating.Interface;
    using ValidatingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Validating.Interface;

    using JobAccessFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Access.Job.Interface;

    using JobResourceFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Resource.Job.Interface;
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;

    public class IntegrationTests : IEnumerable<IntegrationTestCase>
    {
        public IEnumerator<IntegrationTestCase> GetEnumerator()
        {
            yield return new IntegrationTestCase("Loading the Dashboard")
            {
                Id = Guid.Parse("dea5bc26-57e5-46b6-9cba-4adfa54c33ed"),
                Client = "Some Client",
                ServiceRegistration = serviceCollection =>
                {
                    serviceCollection.AddScoped<DashboardManagerFacet.IDashboardManager, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Dashboard.Service.DashboardManager>();
                    serviceCollection.AddScoped<RegulatingEngineFacet.IRegulatingEngine, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating.Service.RegulatingEngine>();
                    serviceCollection.AddScoped<ValidatingEngineFacet.IValidatingEngine, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Validating.Service.ValidatingEngine>();
                    serviceCollection.AddScoped<JobAccessFacet.IJobAccess, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Access.Job.Service.JobAccess>();
                    serviceCollection.AddScoped<JobResourceFacet.IJobResource, CoarseSoftware.Testing.Framework.Examples.Test.System.Resource.Job.Service.JobResource>();
                },
                Service = new IntegrationTestCase.Microservice
                {
                    FacetType = typeof(DashboardManagerFacet.IDashboardManager),
                    MethodName = "FlowAsync",
                    Request = new Test.System.iFX.Request<DashboardManagerFacet.OnStepCompleteBase>
                    {
                        Data = new DashboardManagerFacet.OnStepCompleteBase
                        { }
                    },
                    ExpectedResponse = new Response<DashboardManagerFacet.OnStepActivateBase>
                    {
                        Data = new DashboardManagerFacet.DerivedOnStepActivate
                        {
                            StaticId = "SomeStaticId"
                        }
                    },
                    IngoredResponsePropertyNames = new List<string>
                    {
                        "RandomId"
                    }
                }
            };

            yield return new IntegrationTestCase("Administration Types Tested")
            {
                Id = Guid.Parse("03e24be7-a0e0-464b-a644-fe39f3707848"),
                Client = "Some Client",
                ServiceRegistration = serviceCollection =>
                {
                    serviceCollection.AddScoped<AdministrationManagerFacet.IAdministrationManager, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Service.AdministrationManager>();
                },
                Service = new IntegrationTestCase.Microservice
                {
                    FacetType = typeof(AdministrationManagerFacet.IAdministrationManager),
                    MethodName = "FlowAsync",
                    Request = new Request<AdministrationManagerFacet.OnStepCompleteBase>
                    {
                        Data = new CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete
                        {
                            DateTimeOffset = DateTimeOffset.Parse("1/1/1999 12:00:00 AM +00:00"),
                            DateTime = DateTime.Parse("1/1/2040 12:00:00 AM"),
                            Bool = true,
                            Int = 69,
                            Decimal = 1.5m,
                            Float = 1.92f,
                            Double = 12.45d,
                            String = "SomeString",
                            Strings = new List<string>
                            {
                                "String1",
                                "String2"
                            },
                            Lists = new List<bool>
                            {
                            },
                            SubClassIgnore = new AdministrationManagerFacet.UseCases.SomeContext.DerivedOnStepComplete.SubClassToIgnore
                            {
                                PropertyToIgnore = "not the same"
                            },
                            SubClassWithDates = new List<AdministrationManagerFacet.UseCases.SomeContext.DerivedOnStepComplete.SubClassWithDate>
                            {
                                new AdministrationManagerFacet.UseCases.SomeContext.DerivedOnStepComplete.SubClassWithDate
                                {
                                    Date = DateTimeOffset.UtcNow
                                },
                                new AdministrationManagerFacet.UseCases.SomeContext.DerivedOnStepComplete.SubClassWithDate
                                {
                                    Date = DateTimeOffset.UtcNow
                                }
                            },
                            SubClasses = new List<CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SubClass>
                            {
                                new CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SubClass
                                {
                                    SomeEnum = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.None,
                                    SomeEnumNull = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.None
                                },
                                new CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SubClass
                                {
                                    SomeEnum = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.Value,
                                    SomeEnumNull = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.None
                                }
                            },
                            SubClassWithValue = new CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SubClass
                            {
                                SomeEnum = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.Value,
                                SomeEnumNull = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.None
                            }
                        }
                    },
                    ExpectedResponse = new Response<AdministrationManagerFacet.OnStepActivateBase>
                    {
                        Data = new AdministrationManagerFacet.OnStepActivateBase
                        {
                            SomeBaseClassField = "Types Tested"
                        }
                    }
                }
            };

            yield return new IntegrationTestCase("Administration Types Tested with null expected microservice response")
            {
                Id = Guid.Parse("b7bf561c-e8e1-4326-97a9-3c6a446e60c5"),
                Client = "Some Client",
                ServiceRegistration = serviceCollection =>
                {
                    serviceCollection.AddScoped<AdministrationManagerFacet.IAdministrationManager, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Service.AdministrationManager>();
                },
                Service = new IntegrationTestCase.Microservice
                {
                    FacetType = typeof(AdministrationManagerFacet.IAdministrationManager),
                    MethodName = "FlowAsync",
                    Request = new Request<AdministrationManagerFacet.OnStepCompleteBase>
                    {
                        Data = new CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete
                        {
                            DateTimeOffset = DateTimeOffset.Parse("1/1/1999 12:00:00 AM +00:00"),
                            DateTime = DateTime.Parse("1/1/2099 12:00:00 AM"),
                            Bool = true,
                            Int = 69,
                            Decimal = 1.5m,
                            Float = 1.92f,
                            Double = 12.45d,
                            String = "SomeString",
                            Strings = new List<string>
                            {
                                "String1",
                                "String2"
                            },
                            Lists = new List<bool>
                            {
                            },
                            SubClassIgnore = new AdministrationManagerFacet.UseCases.SomeContext.DerivedOnStepComplete.SubClassToIgnore
                            {
                                PropertyToIgnore = "not the same"
                            },
                            SubClassWithDates = new List<AdministrationManagerFacet.UseCases.SomeContext.DerivedOnStepComplete.SubClassWithDate>
                            {
                                new AdministrationManagerFacet.UseCases.SomeContext.DerivedOnStepComplete.SubClassWithDate
                                {
                                    Date = DateTimeOffset.UtcNow
                                },
                                new AdministrationManagerFacet.UseCases.SomeContext.DerivedOnStepComplete.SubClassWithDate
                                {
                                    Date = DateTimeOffset.UtcNow
                                }
                            },
                            SubClasses = new List<CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SubClass>
                            {
                                new CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SubClass
                                {
                                    SomeEnum = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.None,
                                    SomeEnumNull = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.None
                                },
                                new CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SubClass
                                {
                                    SomeEnum = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.Value,
                                    SomeEnumNull = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.None
                                }
                            },
                            SubClassWithValue = new CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SubClass
                            {
                                SomeEnum = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.Value,
                                SomeEnumNull = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete.SomeEnum.None
                            }
                        }
                    },
                    ExpectedResponse = new Response<AdministrationManagerFacet.OnStepActivateBase>
                    {
                        Data = new AdministrationManagerFacet.OnStepActivateBase
                        {
                            SomeBaseClassField = "Types Tested"
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

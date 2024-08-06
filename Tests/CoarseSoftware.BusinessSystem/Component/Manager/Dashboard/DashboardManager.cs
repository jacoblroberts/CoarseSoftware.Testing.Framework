namespace CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Service
{
    using CoarseSoftware.BusinessSystem.Component.Engine.Transforming;
    using CoarseSoftware.BusinessSystem.iFX;
    using Microsoft.Extensions.DependencyInjection;
    using RegulatingEngineFacet = CoarseSoftware.BusinessSystem.Component.Engine.Regulating.Interface;
    using TransformingEngineFacet = CoarseSoftware.BusinessSystem.Component.Engine.Transforming.Interface;
    using ValidatingEngineFacet = CoarseSoftware.BusinessSystem.Component.Engine.Validating.Interface;
    using JobAccessFacet = CoarseSoftware.BusinessSystem.Component.Access.Job.Interface;
    using CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Event;
    using CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Event.Model;
    using CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface;

    public class DashboardManager : IDashboardManager, IMessageEventListener<SomeCrossVolatilityMessageEvent>
    {
        private readonly IServiceProvider serviceProvider;
        public DashboardManager(
            IServiceProvider serviceProvider
            )
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<Response<OnStepActivateBase>> FlowAsync(Request<OnStepCompleteBase> stepComplete, CancellationToken cancellationToken)
        {
            var regulatingEngine = serviceProvider.GetService<RegulatingEngineFacet.IRegulatingEngine>();
            var validatingEngine = serviceProvider.GetService<ValidatingEngineFacet.IValidatingEngine>();

            var numbers = new[]
            {
                1, 2, 3, 6
            };

            var regulatingEngineResponse = await regulatingEngine.ApplyAsync(new Request<RegulatingEngineFacet.RequestBase>
            {
                Data = new RegulatingEngineFacet.RequestBase
                {
                    Numbers = numbers.Select(n => n)
                }
            }, cancellationToken);

            if (regulatingEngineResponse.Data.Number == 1)
            {
                return new Response<OnStepActivateBase>
                {
                    Data = new DerivedOnStepActivate
                    {
                        RandomId = Guid.NewGuid().ToString(),
                        StaticId = "IdNumber1"
                    }
                };
            }
            else if (regulatingEngineResponse.Data.Number == 2)
            {
                return new Response<OnStepActivateBase>
                {
                    Data = new DerivedOnStepActivate
                    {
                        RandomId = Guid.NewGuid().ToString(),
                        StaticId = "IdNumber2"
                    }
                };
            }

            var validatingEngineResponse = await validatingEngine.ValidateAsync(new Request<ValidatingEngineFacet.RequestBase>
            {
                Data = new ValidatingEngineFacet.RequestBase { }
            }, cancellationToken);

            if (validatingEngineResponse.Data.IsValid)
            {
                return new Response<OnStepActivateBase>
                {
                    Data = new DerivedOnStepActivate
                    {
                        RandomId = Guid.NewGuid().ToString(),
                        StaticId = "SomeStaticId"
                    }
                };
            }

            var transformingEngine = serviceProvider.GetService<TransformingEngineFacet.ITransformingEngine>();
            await transformingEngine.TransformAsync(new Request<TransformingEngineFacet.RequestBase>
            {
                Data = new TransformingEngineFacet.RequestBase
                { }
            }, cancellationToken);

            var jobAccess = this.serviceProvider.GetService<JobAccessFacet.IJobAccess>();
            var jobAccessResponse = jobAccess.FilterAsync(new Request<JobAccessFacet.RequestBase>
            {
                Data = new JobAccessFacet.RequestBase
                { }
            }, cancellationToken);

            return new Response<OnStepActivateBase>
            {
                Data = new OnStepActivateBase
                { }
            };
        }

        async Task IMessageEventListener<SomeCrossVolatilityMessageEvent>.ProcessAsync(SomeCrossVolatilityMessageEvent message, CancellationToken cancellationToken)
        {
            await Task.Yield();
        }
    }
}

using CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Transforming;
using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;
using Microsoft.Extensions.DependencyInjection;
using RegulatingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating;
using TransformingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Transforming;
using ValidatingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Validating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Dashboard
{
    public class DashboardManager : IDashboardManager
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

            var transformingEngine = serviceProvider.GetService<ITransformingEngine>();
            await transformingEngine.TransformAsync(new Request<TransformingEngineFacet.RequestBase>
            {
                Data = new TransformingEngineFacet.RequestBase
                { }
            }, cancellationToken);

            return new Response<OnStepActivateBase>
            {
                Data = new OnStepActivateBase
                { }
            };
        }
    }
}

namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Transforming.Service
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Transforming.Interface;
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;
    using Microsoft.Extensions.DependencyInjection;
    using JobAccessFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Access.Job.Interface;

    public class TransformingEngine : ITransformingEngine
    {
        private readonly IServiceProvider serviceProvider;
        public TransformingEngine(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<Response<ResponseBase>> TransformAsync(Request<RequestBase> request, CancellationToken cancellationToken)
        {
            var jobAccess = this.serviceProvider.GetService<JobAccessFacet.IJobAccess>();
            var jobAccessResponse = jobAccess.FilterAsync(new Request<JobAccessFacet.RequestBase>
            {
                Data = new JobAccessFacet.RequestBase
                { }
            }, cancellationToken);

            return new Response<ResponseBase>
            {
                Data = new ResponseBase
                { }
            };
        }
    }
}

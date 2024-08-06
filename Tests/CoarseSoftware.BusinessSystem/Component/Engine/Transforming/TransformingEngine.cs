namespace CoarseSoftware.BusinessSystem.Component.Engine.Transforming.Service
{
    using CoarseSoftware.BusinessSystem.Component.Engine.Transforming.Interface;
    using CoarseSoftware.BusinessSystem.iFX;
    using Microsoft.Extensions.DependencyInjection;
    using JobAccessFacet = CoarseSoftware.BusinessSystem.Component.Access.Job.Interface;

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

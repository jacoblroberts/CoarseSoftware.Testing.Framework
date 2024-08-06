namespace CoarseSoftware.BusinessSystem.Component.Engine.Regulating.Service
{
    using CoarseSoftware.BusinessSystem.Component.Engine.Regulating.Interface;
    using CoarseSoftware.BusinessSystem.iFX;
    using Microsoft.Extensions.DependencyInjection;
    using JobAccessFacet = CoarseSoftware.BusinessSystem.Component.Access.Job.Interface;

    public class RegulatingEngine : IRegulatingEngine
    {
        private readonly IServiceProvider serviceProvider;

        public RegulatingEngine(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;    
        }

        public async Task<Response<ResponseBase>> ApplyAsync(Request<RequestBase> request, CancellationToken cancellationToken)
        {
            var jobAccess = serviceProvider.GetService<JobAccessFacet.IJobAccess>();
            var jobAccessResponse = await jobAccess.FilterAsync(new Request<JobAccessFacet.RequestBase>
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

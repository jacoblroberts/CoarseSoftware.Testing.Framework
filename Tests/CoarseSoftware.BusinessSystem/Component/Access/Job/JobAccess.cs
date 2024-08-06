namespace CoarseSoftware.BusinessSystem.Component.Access.Job.Service
{
    using CoarseSoftware.BusinessSystem.Component.Access.Job.Interface;
    using CoarseSoftware.BusinessSystem.iFX;
    using Microsoft.Extensions.DependencyInjection;

    using JobResourceFacet = CoarseSoftware.BusinessSystem.Resource.Job.Interface;
    public class JobAccess : IJobAccess
    {
        private readonly IServiceProvider serviceProvider;
        public JobAccess(
            IServiceProvider serviceProvider
            )
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<Response<ResponseBase>> FilterAsync(Request<RequestBase> request, CancellationToken cancellationToken)
        {
            var jobResource = serviceProvider.GetService<JobResourceFacet.IJobResource>();

            var jobResourceResponse = await jobResource.ListAsync(new Request<JobResourceFacet.RequestBase>
            {
                Data = new JobResourceFacet.RequestBase
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

namespace CoarseSoftware.BusinessSystem.Resource.Job.Service
{
    using CoarseSoftware.BusinessSystem.iFX;
    using CoarseSoftware.BusinessSystem.Resource.Job.Interface;

    public class JobResource: IJobResource
    {
        private readonly IServiceProvider serviceProvider;
        public JobResource(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<Response<ResponseBase>> ListAsync(Request<RequestBase> request, CancellationToken cancellationToken)
        {
            await Task.Yield();
            return new Response<ResponseBase>
            {
                Data = new ResponseBase
                { }
            };
        }
    }
}

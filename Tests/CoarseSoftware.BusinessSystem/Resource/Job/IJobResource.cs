namespace CoarseSoftware.BusinessSystem.Resource.Job.Interface
{
    using CoarseSoftware.BusinessSystem.iFX;

    public interface IJobResource
    {
        Task<Response<ResponseBase>> ListAsync(Request<RequestBase> request, CancellationToken cancellationToken);
    }

    public class ResponseBase
    { }

    public class RequestBase
    { }
}

namespace CoarseSoftware.BusinessSystem.Component.Access.Job.Interface
{
    using CoarseSoftware.BusinessSystem.iFX;

    public interface IJobAccess
    {
        Task<Response<ResponseBase>> FilterAsync(Request<RequestBase> request, CancellationToken cancellationToken);
    }

    public class ResponseBase
    { }

    public class RequestBase
    { }
}

namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Resource.Job.Interface
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;

    public interface IJobResource
    {
        Task<Response<ResponseBase>> ListAsync(Request<RequestBase> request, CancellationToken cancellationToken);
    }

    public class ResponseBase
    { }

    public class RequestBase
    { }
}

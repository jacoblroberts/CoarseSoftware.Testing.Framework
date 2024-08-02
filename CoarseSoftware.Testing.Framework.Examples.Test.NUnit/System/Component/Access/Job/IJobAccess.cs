namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Access.Job.Interface
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;

    public interface IJobAccess
    {
        Task<Response<ResponseBase>> FilterAsync(Request<RequestBase> request, CancellationToken cancellationToken);
    }

    public class ResponseBase
    { }

    public class RequestBase
    { }
}

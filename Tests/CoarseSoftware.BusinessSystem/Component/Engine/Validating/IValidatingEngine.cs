namespace CoarseSoftware.BusinessSystem.Component.Engine.Validating.Interface
{
    using CoarseSoftware.BusinessSystem.iFX;

    public interface IValidatingEngine
    {
        Task<Response<ResponseBase>> ValidateAsync(Request<RequestBase> request, CancellationToken cancellationToken);
    }

    public class RequestBase { }
    public class ResponseBase
    {
        public bool IsValid { get; set; }
    }
}

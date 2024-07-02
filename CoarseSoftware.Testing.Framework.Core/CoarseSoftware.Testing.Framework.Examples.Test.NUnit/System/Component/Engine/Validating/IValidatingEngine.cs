namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Validating
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;

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

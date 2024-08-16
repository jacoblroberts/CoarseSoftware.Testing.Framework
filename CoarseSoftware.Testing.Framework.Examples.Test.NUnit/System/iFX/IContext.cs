namespace CoarseSoftware.Testing.Framework.Examples.Test.System.iFX
{
    public interface IContext
    {
        // auth token, request context (like localization, client...)
        string SomeId { get; set; }
    }

    public class RequestContext : IContext
    {
        public string SomeId { get; set; }
    }
}

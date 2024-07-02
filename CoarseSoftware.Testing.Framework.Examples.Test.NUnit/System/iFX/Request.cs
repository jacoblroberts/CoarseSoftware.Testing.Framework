namespace CoarseSoftware.Testing.Framework.Examples.Test.System.iFX
{
    public class Request<T>
    {
        public IContext Context { get; set; }
        public T Data { get; set; }
    }
}

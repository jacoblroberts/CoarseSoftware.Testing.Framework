namespace CoarseSoftware.BusinessSystem.iFX
{
    public class Request<T>
    {
        public IContext Context { get; set; }
        public T Data { get; set; }
    }
}

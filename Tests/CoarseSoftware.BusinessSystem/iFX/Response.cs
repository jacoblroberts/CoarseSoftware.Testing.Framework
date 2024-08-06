namespace CoarseSoftware.BusinessSystem.iFX
{
    public class Response<T>
    {
        public IContext Context { get; set; }
        public T Data { get; set; }
    }
}

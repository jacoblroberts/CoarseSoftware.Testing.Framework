namespace CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Event
{
    public interface IMessageEventListener<T> where T : MessageEventBase
    {
        Task ProcessAsync(T message, CancellationToken cancellationToken);
    }
}

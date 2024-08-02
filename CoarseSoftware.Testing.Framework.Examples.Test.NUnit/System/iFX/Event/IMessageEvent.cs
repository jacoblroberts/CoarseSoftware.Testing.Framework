namespace CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Event
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;

    public interface IMessageEvent<T> where T: MessageEventBase
    {
        Task PublishAsync(T message, CancellationToken cancellationToken);
    }

    public class MessageEventBase
    {
        public IContext Context { get; set; }
    }
}

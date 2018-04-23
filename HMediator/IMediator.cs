namespace HMediator
{
    public interface IMediator
    {
        void Send(ICommand cmd);
        TResult Send<TResult>(IQuery<TResult> cmd);
    }
}

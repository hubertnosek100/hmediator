namespace HMediator
{
    public interface ICommandHandler<T>
    {
        void Handle(T request);
    }
}
namespace HMediator
{
    public interface IQueryHandler<T,TResult>
    {
        TResult Handle(T request);
    }
}
